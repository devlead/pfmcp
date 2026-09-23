namespace Pfmcp.Services.Pagefind;

internal static partial class PagefindCodec
{
    public static PagefindEntry DecodeEntry(ReadOnlySpan<byte> json)
        => JsonSerializer.Deserialize<PagefindEntry>(json, JsonOptions)
           ?? throw new InvalidDataException("pagefind-entry.json was empty.");

    public static PageFragment DecodeFragment(byte[] bytes)
    {
        var json = GzipUtil.DecompressIfNeeded(bytes);
        return JsonSerializer.Deserialize<PageFragment>(json, JsonOptions)
               ?? throw new InvalidDataException("Fragment JSON was empty.");
    }

    public static byte[] EncodeFragment(PageFragment fragment)
        => GzipUtil.Compress(JsonSerializer.SerializeToUtf8Bytes(fragment, JsonOptions));
}
