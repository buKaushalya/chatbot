using RagApi.Core.Records;

namespace RagApi.Services.Ingestion;

public interface IChunker
{
    IReadOnlyList<TextChunk> Chunk(string text);
}