namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PageDocument
{
    public required string Index { get; init; }
    public required string PageId { get; init; }
    public required string Url { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required Dictionary<string, string> Meta { get; init; }
    public required IReadOnlyList<PageAnchor> Anchors { get; init; }
    public bool Truncated { get; init; }
}
