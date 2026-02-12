using RagApi.Core.Records;

namespace RagApi.Database.Repositories;
public interface IDocumentRepository
{
    Task<(Guid DocumentId, int ChunkCount)> InsertDocumentWithChunksAsync(
        Guid collectionId,
        string title,
        string? source,
        string? contentHash,
        Dictionary<string, object>? documentMetadata,
        IReadOnlyList<ChunkToInsert> chunks);
}