namespace Pfmcp.Services.Pagefind.Models;

internal sealed class LanguageSummary
{
    public required string Language { get; init; }
    public required int PageCount { get; init; }
    public required int ChunkCount { get; init; }
    public required int FilterCount { get; init; }
}
