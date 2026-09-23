namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PackedPage
{
    public uint PageNumber { get; init; }
    public List<(byte Weight, uint Location)> Locs { get; init; } = [];
    public List<(ushort FieldId, uint Location)> MetaLocs { get; init; } = [];
}
