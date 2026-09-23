namespace Pfmcp.Services.Pagefind.Models;

internal sealed class FilterIndex
{
    public required string Filter { get; init; }
    public Dictionary<string, List<uint>> Values { get; init; } = [];
}
