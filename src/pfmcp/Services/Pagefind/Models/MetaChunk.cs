namespace Pfmcp.Services.Pagefind.Models;

internal sealed class MetaChunk
{
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Hash { get; init; }
}
