namespace RagApi.Core.Records;
public record ChunkToInsert(int ChunkIndex, string Content, int? TokenCount, Dictionary<string, object> Metadata, float[] Embedding);