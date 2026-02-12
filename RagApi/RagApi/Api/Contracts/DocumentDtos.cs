namespace RagApi.Api.Contracts;
public class UploadTextDocumentRequest
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Source { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public record UploadTextDocumentResponse(Guid DocumentId, int ChunkCount);