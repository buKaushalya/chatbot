namespace RagApi.Api.Contracts;
public sealed class ChatRequest
{
    public Guid CollectionId { get; set; }
    public Guid? ConversationId { get; set; }
    public string Message { get; set; } = "";
    public int TopK { get; set; } = 6;
}

public sealed record Citation(Guid ChunkId, Guid DocumentId, int ChunkIndex, double Score);

public sealed record ChatResponse(Guid ConversationId, string Answer, IReadOnlyList<Citation> Citations);