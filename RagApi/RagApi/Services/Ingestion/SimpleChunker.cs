using RagApi.Core.Records;

namespace RagApi.Services.Ingestion;
public sealed class SimpleChunker : IChunker
{
    // - chunk by characters with overlap
    // - avoids cutting too aggressively
    public IReadOnlyList<TextChunk> Chunk(string text)
    {
        const int chunkSize = 1500; // chars
        const int overlap = 200;

        text = text.Replace("\r\n", "\n").Trim();
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<TextChunk>();

        var chunks = new List<TextChunk>();
        int i = 0;
        int idx = 0;

        while (i < text.Length)
        {
            idx++;
            var len = Math.Min(chunkSize, text.Length - i);
            var piece = text.Substring(i, len).Trim();

            if (!string.IsNullOrWhiteSpace(piece))
                chunks.Add(new TextChunk(idx, piece));

            i += (chunkSize - overlap);
            if (i < 0) break;
        }

        return chunks;
    }
}
