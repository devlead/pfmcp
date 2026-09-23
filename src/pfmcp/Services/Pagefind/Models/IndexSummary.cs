namespace Pfmcp.Services.Pagefind.Models;

internal sealed class IndexSummary
{
    public required string Name { get; init; }
    public required string Source { get; init; }
    public required string Version { get; init; }
    public required IReadOnlyList<LanguageSummary> Languages { get; init; }
}
