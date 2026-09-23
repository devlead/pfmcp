namespace Pfmcp.Services.Pagefind.Models;

internal sealed class RankingWeights
{
    public float TermSimilarity { get; init; } = 1.0f;
    public float PageLength { get; init; } = 0.75f;
    public float TermSaturation { get; init; } = 1.4f;
    public float TermFrequency { get; init; } = 1.0f;
    public float DiacriticSimilarity { get; init; } = 0.8f;
    public Dictionary<string, float> MetaWeights { get; init; } = new(StringComparer.Ordinal)
    {
        ["title"] = 5.0f
    };
}
