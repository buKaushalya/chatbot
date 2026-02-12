using RagApi.Api.Workloads;

namespace RagApi.Api.Endpoints;
public static class DocumentsEndpoints
{
    public static void MapDocumentEndpoints(this WebApplication app)
    {
        app.MapPost("/v1/collections", DocumentsWorkload.CreateCollection);
        app.MapPost("/v1/collections/{collectionId:guid}/documents/text", DocumentsWorkload.UploadTextDocument);
        app.MapPost("/v1/collections/{collectionId:guid}/search", DocumentsWorkload.Search);
    }
}

