namespace RagApi.Api.Contracts;
public sealed class SearchRequest
{
    public string Query { get; set; } = "";
    public int TopK { get; set; } = 5;
}

public sealed record SearchHit(
    Guid ChunkId,
    Guid DocumentId,
    int ChunkIndex,
    double Score,
    string Content,
    Dictionary<string, object>? Metadata
);

public sealed record SearchResponse(
    string Query,
    int TopK,
    IReadOnlyList<SearchHit> Hits
);