namespace RagApi.Core.Options;

public sealed class EmbeddingOptions
{
    public int Dimension { get; init; } = 384;
    public string ModelPath { get; init; } = "";
    public string TokenizerPath { get; init; } = "";
}