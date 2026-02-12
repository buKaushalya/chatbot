using Dapper;

namespace RagApi.Database.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly IDbConnectionFactory _db;

    public CollectionRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Guid> CreateAsync(string name, string? description)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync();

        var id = await conn.ExecuteScalarAsync<Guid>(
            @"INSERT INTO collections(name, description)
              VALUES (@name, @desc)
              ON CONFLICT (name) DO UPDATE SET description = EXCLUDED.description
              RETURNING id;",
            new { name, desc = description });

        return id;
    }

    public async Task<bool> ExistsAsync(Guid collectionId)
    {
        await using var conn = _db.Create();
        await conn.OpenAsync();

        var exists = await conn.ExecuteScalarAsync<int>(
            @"SELECT 1 FROM collections WHERE id=@id;",
            new { id = collectionId });

        return exists == 1;
    }
}
