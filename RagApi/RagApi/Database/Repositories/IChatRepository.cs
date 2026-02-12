namespace RagApi.Database.Repositories;
public interface IChatRepository
{
    Task<Guid> CreateConversationAsync(Guid collectionId, string? title = null);
    Task<Guid> InsertMessageAsync(Guid conversationId, string role, string content);
    Task InsertRetrievalAsync(Guid messageId, Guid chunkId, int rank, double score);
}