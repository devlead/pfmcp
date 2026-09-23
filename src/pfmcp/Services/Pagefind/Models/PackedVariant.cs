namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PackedVariant
{
    public required string Form { get; init; }
    public List<PackedPage> Pages { get; init; } = [];
}
