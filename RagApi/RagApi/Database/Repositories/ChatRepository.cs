using Dapper;

namespace RagApi.Database.Repositories;

public sealed class ChatRepository : IChatRepository
{
    private readonly IDbConnectionFactory _db;
    public ChatRepository(IDbConnectionFactory db) => _db = db;

    public async Task<Guid> CreateConversationAsync(Guid collectionId, string? title = null)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync();

        return await conn.ExecuteScalarAsync<Guid>(
            @"INSERT INTO conversations(collection_id, title)
              VALUES (@collectionId, @title)
              RETURNING id;",
            new { collectionId, title });
    }

    public async Task<Guid> InsertMessageAsync(Guid conversationId, string role, string content)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync();

        return await conn.ExecuteScalarAsync<Guid>(
            @"INSERT INTO messages(conversation_id, role, content)
              VALUES (@conversationId, @role, @content)
              RETURNING id;",
            new { conversationId, role, content });
    }

    public async Task InsertRetrievalAsync(Guid messageId, Guid chunkId, int rank, double score)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync();

        await conn.ExecuteAsync(
            @"INSERT INTO message_retrievals(message_id, chunk_id, rank, score)
              VALUES (@messageId, @chunkId, @rank, @score)
              ON CONFLICT (message_id, chunk_id) DO NOTHING;",
            new { messageId, chunkId, rank, score });
    }
}
