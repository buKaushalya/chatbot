using Dapper;

namespace RagApi.Database.Repositories;

public sealed class SearchRepository : ISearchRepository
{
    private readonly IDbConnectionFactory _db;
    public SearchRepository(IDbConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<(Guid ChunkId, Guid DocumentId, int ChunkIndex, double Score, string Content, string MetadataJson)>>
        SearchAsync(Guid collectionId, string embeddingVectorLiteral, int topK)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync();

        // cosine distance operator: <=>  (smaller is closer)
        // score here = 1 - distance, so higher is better
        var sql = @"
SELECT
  c.id            AS ChunkId,
  c.doc_id        AS DocumentId,
  c.chunk_index   AS ChunkIndex,
  (1 - (c.embedding <=> @emb::vector))::float8 AS Score,
  c.content       AS Content,
  c.metadata::text AS MetadataJson
FROM chunks c
JOIN documents d ON d.id = c.doc_id
WHERE d.collection_id = @collectionId
ORDER BY c.embedding <=> @emb::vector
LIMIT @topK;
";

        var rows = await conn.QueryAsync(sql, new
        {
            collectionId,
            emb = embeddingVectorLiteral,
            topK
        });

        // Dapper maps to dynamic; convert safely
        var result = new List<(Guid, Guid, int, double, string, string)>();
        foreach (var r in rows)
        {
            result.Add((
                (Guid)r.chunkid,
                (Guid)r.documentid,
                (int)r.chunkindex,
                (double)r.score,
                (string)r.content,
                (string)r.metadatajson
            ));
        }

        return result;
    }
}