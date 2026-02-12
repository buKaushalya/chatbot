namespace RagApi.Services.Ingestion;
public interface IDocumentIngestionService
{
    Task<(Guid DocumentId, int ChunkCount)> IngestTextAsync(
        Guid collectionId,
        string title,
        string content,
        string? source,
        Dictionary<string, object>? metadata,
        CancellationToken ct = default);
}