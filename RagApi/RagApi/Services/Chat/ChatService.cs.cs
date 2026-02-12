using RagApi.Api.Contracts;
using RagApi.Database.Repositories;
using RagApi.Services.Ingestion;

namespace RagApi.Services.Chat;
public sealed class ChatService : IChatService
{
    private readonly ISearchService _search;
    private readonly IChatRepository _chatRepo;
    private readonly IOllamaClient _llm;

    public ChatService(ISearchService search, IChatRepository chatRepo, IOllamaClient llm)
    {
        _search = search;
        _chatRepo = chatRepo;
        _llm = llm;
    }

    public async Task<ChatResponse> ChatAsync(ChatRequest req, CancellationToken ct = default)
    {
        if (req.CollectionId == Guid.Empty) throw new ArgumentException("CollectionId is required.");
        if (string.IsNullOrWhiteSpace(req.Message)) throw new ArgumentException("Message is required.");

        var conversationId = req.ConversationId ?? await _chatRepo.CreateConversationAsync(req.CollectionId, "Chat");
        var userMsgId = await _chatRepo.InsertMessageAsync(conversationId, "user", req.Message);

        var searchResp = await _search.SearchAsync(req.CollectionId, new SearchRequest
        {
            Query = req.Message,
            TopK = req.TopK <= 0 ? 6 : Math.Min(req.TopK, 12)
        }, ct);

        for (int i = 0; i < searchResp.Hits.Count; i++)
        {
            var h = searchResp.Hits[i];
            await _chatRepo.InsertRetrievalAsync(userMsgId, h.ChunkId, i + 1, h.Score);
        }

        var context = string.Join("\n\n",
            searchResp.Hits.Select((h, i) =>
                $"[Source {i + 1}] (doc={h.DocumentId}, chunk={h.ChunkIndex})\n{h.Content}"));

        var prompt = $"""
You are a helpful assistant. Answer the user's question using ONLY the provided sources.
If the answer is not in the sources, say you don't know.

SOURCES:
{context}

QUESTION:
{req.Message}

ANSWER:
""";

        var answer = await _llm.CompleteAsync(prompt, ct);
        await _chatRepo.InsertMessageAsync(conversationId, "assistant", answer);

        var citations = searchResp.Hits
            .Select(h => new Citation(h.ChunkId, h.DocumentId, h.ChunkIndex, h.Score))
            .ToList();

        return new ChatResponse(conversationId, answer, citations);
    }
}