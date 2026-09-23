namespace Pfmcp.Services.Pagefind.Models;

internal sealed class SearchHit
{
    public required string Index { get; init; }
    public required string Language { get; init; }
    public required string PageId { get; init; }
    public required string Url { get; init; }
    public required string Title { get; init; }
    public required string Excerpt { get; init; }
    public required float Score { get; init; }
}
