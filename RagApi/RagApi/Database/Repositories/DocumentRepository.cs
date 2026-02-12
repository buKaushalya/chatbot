using Dapper;
using System.Text.Json;
using RagApi.Core.Records;

namespace RagApi.Database.Repositories;
public sealed class DocumentRepository : IDocumentRepository
{
    private readonly IDbConnectionFactory _db;
    public DocumentRepository(IDbConnectionFactory db) => _db = db;

    public async Task<(Guid DocumentId, int ChunkCount)> InsertDocumentWithChunksAsync(
        Guid collectionId,
        string title,
        string? source,
        string? contentHash,
        Dictionary<string, object>? documentMetadata,
        IReadOnlyList<ChunkToInsert> chunks)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        var docMetaJson = JsonSerializer.Serialize(documentMetadata ?? new Dictionary<string, object>());

        var documentId = await conn.ExecuteScalarAsync<Guid>(
            @"INSERT INTO documents(collection_id, title, source, content_hash, metadata)
              VALUES (@collectionId, @title, @source, @contentHash, CAST(@meta AS jsonb))
              RETURNING id;",
            new { collectionId, title, source, contentHash, meta = docMetaJson }, tx);

        foreach (var ch in chunks)
        {
            // pgvector literal: [0.1,0.2,...]
            var vec = "[" + string.Join(",", ch.Embedding.Select(v => v.ToString("G", System.Globalization.CultureInfo.InvariantCulture))) + "]";

            var chunkMetaJson = JsonSerializer.Serialize(ch.Metadata ?? new Dictionary<string, object>());

            await conn.ExecuteAsync(
                @"INSERT INTO chunks(doc_id, chunk_index, content, token_count, metadata, embedding)
                  VALUES (@docId, @chunkIndex, @content, @tokenCount, CAST(@meta AS jsonb), @embedding::vector);",
                new
                {
                    docId = documentId,
                    chunkIndex = ch.ChunkIndex,
                    content = ch.Content,
                    tokenCount = ch.TokenCount,
                    meta = chunkMetaJson,
                    embedding = vec
                }, tx);
        }

        await tx.CommitAsync();
        return (documentId, chunks.Count);
    }
}