using RagApi.Core.Records;
using RagApi.Database.Repositories;
using RagApi.Services.Embedding;
using System.Security.Cryptography;
using System.Text;

namespace RagApi.Services.Ingestion;

public sealed class DocumentIngestionService : IDocumentIngestionService
{
    private readonly IChunker _chunker;
    private readonly IEmbeddingService _embedder;
    private readonly IDocumentRepository _repo;

    public DocumentIngestionService(IChunker chunker, IEmbeddingService embedder, IDocumentRepository repo)
    {
        _chunker = chunker;
        _embedder = embedder;
        _repo = repo;
    }

    public async Task<(Guid DocumentId, int ChunkCount)> IngestTextAsync(
        Guid collectionId,
        string title,
        string content,
        string? source,
        Dictionary<string, object>? metadata,
        CancellationToken ct = default)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        var chunks = _chunker.Chunk(content);
        var toInsert = new List<ChunkToInsert>(chunks.Count);

        foreach (var c in chunks)
        {
            var emb = await _embedder.EmbedAsync(c.Text, ct);

            toInsert.Add(new ChunkToInsert(
                ChunkIndex: c.Index,
                Content: c.Text,
                TokenCount: null,
                Metadata: new Dictionary<string, object> { ["chunk"] = c.Index },
                Embedding: emb));
        }

        return await _repo.InsertDocumentWithChunksAsync(
            collectionId,
            title,
            source,
            hash,
            metadata,
            toInsert);
    }
}