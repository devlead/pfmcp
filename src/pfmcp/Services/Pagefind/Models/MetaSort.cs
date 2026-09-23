namespace Pfmcp.Services.Pagefind.Models;

internal sealed class MetaSort
{
    public required string Sort { get; init; }
    public List<uint> Pages { get; init; } = [];
}
