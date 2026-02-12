using RagApi.Api.Contracts;
using RagApi.Database.Repositories;
using RagApi.Services.Ingestion;

namespace RagApi.Api.Workloads;
public static class DocumentsWorkload
{
    public static async Task<IResult> CreateCollection(CreateCollectionRequest req, ICollectionRepository repo) 
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest("Collection name is required.");

        var id = await repo.CreateAsync(req.Name.Trim(), req.Description?.Trim());
        return Results.Ok(new CreateCollectionResponse(id));
    }

    public static async Task<IResult> UploadTextDocument(Guid collectionId,
           UploadTextDocumentRequest req,
           ICollectionRepository collections,
           IDocumentIngestionService ingestion,
           CancellationToken ct)
    {
        if (!await collections.ExistsAsync(collectionId))
            return Results.NotFound("Collection not found.");

        if (string.IsNullOrWhiteSpace(req.Title))
            return Results.BadRequest("Title is required.");

        if (string.IsNullOrWhiteSpace(req.Content))
            return Results.BadRequest("Content is required.");

        var (docId, chunkCount) = await ingestion.IngestTextAsync(
            collectionId,
            req.Title.Trim(),
            req.Content,
            req.Source,
            req.Metadata,
            ct);

        return Results.Ok(new UploadTextDocumentResponse(docId, chunkCount));
    }

    public static async Task<IResult> Search(Guid collectionId,
           SearchRequest req,
           ICollectionRepository collections,
           RagApi.Services.Ingestion.ISearchService search,
           CancellationToken ct)
    {
        if (!await collections.ExistsAsync(collectionId))
            return Results.NotFound("Collection not found.");

        try
        {
            var resp = await search.SearchAsync(collectionId, req, ct);
            return Results.Ok(resp);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}

