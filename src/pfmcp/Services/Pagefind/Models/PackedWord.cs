namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PackedWord
{
    public required string Word { get; init; }
    public List<PackedPage> Pages { get; init; } = [];
    public List<PackedVariant> AdditionalVariants { get; init; } = [];
}
