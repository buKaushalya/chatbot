namespace RagApi.Services.Chat;
public interface IOllamaClient
{
    Task<string> CompleteAsync(string prompt, CancellationToken ct = default);
}