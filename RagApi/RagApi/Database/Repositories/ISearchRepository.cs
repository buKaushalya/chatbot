namespace RagApi.Database.Repositories;
public interface ISearchRepository
{
    Task<IReadOnlyList<(Guid ChunkId, Guid DocumentId, int ChunkIndex, double Score, string Content, string MetadataJson)>>
        SearchAsync(Guid collectionId, string embeddingVectorLiteral, int topK);
}