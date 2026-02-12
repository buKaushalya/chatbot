namespace RagApi.Api.Contracts;
public record CreateCollectionRequest(string Name, string? Description);

public record CreateCollectionResponse(Guid CollectionId);
