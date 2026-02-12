using RagApi.Api.Contracts;
using RagApi.Database.Repositories;
using RagApi.Services.Embedding;
using System.Text.Json;

namespace RagApi.Services.Ingestion;

public sealed class SearchService : ISearchService
{
    private readonly IEmbeddingService _embedder;
    private readonly ISearchRepository _repo;

    public SearchService(IEmbeddingService embedder, ISearchRepository repo)
    {
        _embedder = embedder;
        _repo = repo;
    }

    public async Task<SearchResponse> SearchAsync(Guid collectionId, SearchRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Query))
            throw new ArgumentException("Query is required.");

        var topK = req.TopK <= 0 ? 5 : Math.Min(req.TopK, 20);

        var vec = await _embedder.EmbedAsync(req.Query, ct);
        var vecLiteral = "[" + string.Join(",", vec.Select(v => v.ToString("G", System.Globalization.CultureInfo.InvariantCulture))) + "]";

        var rows = await _repo.SearchAsync(collectionId, vecLiteral, topK);

        var hits = rows.Select(r =>
        {
            Dictionary<string, object>? meta = null;
            try
            {
                meta = JsonSerializer.Deserialize<Dictionary<string, object>>(r.MetadataJson);
            }
            catch { /* ignore */ }

            return new SearchHit(
                r.ChunkId,
                r.DocumentId,
                r.ChunkIndex,
                r.Score,
                r.Content,
                meta
            );
        }).ToList();

        return new SearchResponse(req.Query, topK, hits);
    }
}