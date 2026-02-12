using RagApi.Api.Contracts;

namespace RagApi.Services.Ingestion;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(Guid collectionId, SearchRequest req, CancellationToken ct = default);
}
