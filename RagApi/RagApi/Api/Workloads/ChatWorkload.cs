using RagApi.Api.Contracts;
using RagApi.Services.Chat;

namespace RagApi.Api.Workloads;
public static class ChatWorkload
{
    public static async Task<IResult> ChatAsync(
        ChatRequest req, 
        IChatService chat, 
        CancellationToken ct)
    {
        try
        {
            var resp = await chat.ChatAsync(req, ct);
            return Results.Ok(resp);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}
