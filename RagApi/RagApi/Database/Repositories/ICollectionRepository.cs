namespace RagApi.Database.Repositories;
public interface ICollectionRepository
{
    Task<Guid> CreateAsync(string name, string? description);
    Task<bool> ExistsAsync(Guid collectionId);
}