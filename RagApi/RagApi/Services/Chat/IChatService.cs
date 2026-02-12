using RagApi.Api.Contracts;

namespace RagApi.Services.Chat;
public interface IChatService
{
    Task<ChatResponse> ChatAsync(ChatRequest req, CancellationToken ct = default);
}