using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using RagApi.Core.Options;
using Tokenizers.HuggingFace.Tokenizer;

namespace RagApi.Services.Embedding;

public sealed class OnnxEmbeddingService : IEmbeddingService, IDisposable
{
    private readonly InferenceSession _session;
    private readonly Tokenizer _tokenizer;
    private readonly int _dim;

    // ONNX input/output names can vary slightly; we discover at runtime.
    private readonly string _inputIdsName;
    private readonly string _attentionMaskName;
    private readonly string? _tokenTypeIdsName;
    private readonly string _outputName;

    public OnnxEmbeddingService(IOptions<EmbeddingOptions> opt, IWebHostEnvironment env)
    {
        var o = opt.Value;
        _dim = o.Dimension;

        var modelPath = Path.IsPathRooted(o.ModelPath) ? o.ModelPath : Path.Combine(env.ContentRootPath, o.ModelPath);
        var tokenizerPath = Path.IsPathRooted(o.TokenizerPath) ? o.TokenizerPath : Path.Combine(env.ContentRootPath, o.TokenizerPath);

        if (!File.Exists(modelPath)) throw new FileNotFoundException("Embedding model.onnx not found", modelPath);
        if (!File.Exists(tokenizerPath)) throw new FileNotFoundException("Embedding tokenizer.json not found", tokenizerPath);

        _tokenizer = Tokenizer.FromFile(tokenizerPath);
        _session = new InferenceSession(modelPath);

        // Inputs
        var inputs = _session.InputMetadata.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _inputIdsName = inputs.FirstOrDefault(x => x.Equals("input_ids", StringComparison.OrdinalIgnoreCase))
                        ?? _session.InputMetadata.Keys.First();
        _attentionMaskName = inputs.FirstOrDefault(x => x.Equals("attention_mask", StringComparison.OrdinalIgnoreCase))
                             ?? _session.InputMetadata.Keys.Skip(1).FirstOrDefault()
                             ?? "attention_mask";

        _tokenTypeIdsName = inputs.FirstOrDefault(x => x.Equals("token_type_ids", StringComparison.OrdinalIgnoreCase));

        // Outputs: prefer rank-3 embedding output (batch, seq, hidden)
        _outputName = PickOutputName(_session);
    }

    public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        const int maxLength = 256;

        // Get encoding
        var encoding = _tokenizer.Encode(text, addSpecialTokens: true).First();

        // Token ids + attention
        var idsRaw = encoding.Ids.Select(i => (long)i).ToArray();
        var attnRaw = encoding.AttentionMask.Select(i => (long)i).ToArray();

        // PAD/TRUNCATE to fixed maxLength
        var inputIds = new DenseTensor<long>(new[] { 1, maxLength });
        var attentionMask = new DenseTensor<long>(new[] { 1, maxLength });

        // Most HF models use 0 as padding id; safe default for MiniLM ONNX packs
        const long padId = 0;

        for (int i = 0; i < maxLength; i++)
        {
            inputIds[0, i] = padId;
            attentionMask[0, i] = 0;
        }

        var len = Math.Min(idsRaw.Length, maxLength);
        for (int i = 0; i < len; i++)
        {
            inputIds[0, i] = idsRaw[i];
            attentionMask[0, i] = (i < attnRaw.Length ? attnRaw[i] : 1);
        }

        var container = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor(_inputIdsName, inputIds),
        NamedOnnxValue.CreateFromTensor(_attentionMaskName, attentionMask),
    };

        if (!string.IsNullOrWhiteSpace(_tokenTypeIdsName))
        {
            var tokenType = new DenseTensor<long>(new[] { 1, maxLength }); // all zeros
            container.Add(NamedOnnxValue.CreateFromTensor(_tokenTypeIdsName!, tokenType));
        }

        using var results = _session.Run(container);
        var output = results.First(r => r.Name == _outputName).AsTensor<float>();

        // output: [1, seqLen, hidden]
        var seqLen = output.Dimensions[1];
        var hidden = output.Dimensions[2];

        if (hidden != _dim)
            throw new InvalidOperationException($"Model output dim={hidden} but configured dim={_dim}.");

        // Mean pooling using attention mask
        var pooled = new float[hidden];
        float denom = 0;

        // seqLen should now equal maxLength, but we'll still be defensive
        var effectiveLen = Math.Min(seqLen, maxLength);

        for (int t = 0; t < effectiveLen; t++)
        {
            var mask = attentionMask[0, t];
            if (mask == 0) continue;

            denom += 1;
            for (int h = 0; h < hidden; h++)
                pooled[h] += output[0, t, h];
        }

        if (denom > 0)
            for (int h = 0; h < hidden; h++)
                pooled[h] /= denom;

        // L2 normalize
        var norm = (float)Math.Sqrt(pooled.Sum(v => v * v));
        if (norm > 0)
            for (int h = 0; h < hidden; h++)
                pooled[h] /= norm;

        return Task.FromResult(pooled);
    }

    private static string PickOutputName(InferenceSession session)
    {
        foreach (var kv in session.OutputMetadata)
        {
            var dims = kv.Value.Dimensions;
            // rank 3: [batch, seq, hidden]
            if (dims.Length == 3)
                return kv.Key;
        }
        return session.OutputMetadata.Keys.First();
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}