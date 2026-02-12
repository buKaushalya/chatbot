using RagApi.Api.Workloads;

namespace RagApi.Api.Endpoints;
public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/v1/chat", ChatWorkload.ChatAsync);
    }
}

