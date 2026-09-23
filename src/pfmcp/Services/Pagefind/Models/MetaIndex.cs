namespace Pfmcp.Services.Pagefind.Models;

internal sealed class MetaIndex
{
    public string Version { get; init; } = "";
    public List<MetaPage> Pages { get; init; } = [];
    public List<MetaChunk> IndexChunks { get; init; } = [];
    public List<MetaFilter> Filters { get; init; } = [];
    public List<MetaSort> Sorts { get; init; } = [];
    public List<string> MetaFields { get; init; } = [];

    public float AveragePageLength =>
        Pages.Count == 0 ? 0 : Pages.Average(p => (float)p.WordCount);
}
