namespace Pfmcp.Services.Pagefind;

internal static partial class PagefindCodec
{
    public static FilterIndex DecodeFilterIndex(byte[] bytes)
    {
        var payload = GzipUtil.DecompressIfNeeded(bytes);
        var reader = new CborReader(payload, CborConformanceMode.Lax);
        CborUtil.ReadArrayLength(reader);
        var filter = reader.ReadTextString();
        var values = new Dictionary<string, List<uint>>(StringComparer.Ordinal);
        var valueCount = CborUtil.ReadArrayLength(reader);
        for (var i = 0; i < valueCount; i++)
        {
            CborUtil.ReadArrayLength(reader);
            var value = reader.ReadTextString();
            var pages = new List<uint>();
            var n = CborUtil.ReadArrayLength(reader);
            for (var p = 0; p < n; p++)
            {
                pages.Add(CborUtil.ReadUInt32(reader));
            }

            reader.ReadEndArray();
            values[value] = pages;
            reader.ReadEndArray();
        }

        reader.ReadEndArray();
        return new FilterIndex { Filter = filter, Values = values };
    }

    public static byte[] EncodeFilterIndex(FilterIndex index)
    {
        var writer = new CborWriter();
        writer.WriteStartArray(2);
        writer.WriteTextString(index.Filter);
        writer.WriteStartArray(index.Values.Count);
        foreach (var (value, pages) in index.Values)
        {
            writer.WriteStartArray(2);
            writer.WriteTextString(value);
            writer.WriteStartArray(pages.Count);
            foreach (var page in pages)
            {
                writer.WriteUInt32(page);
            }

            writer.WriteEndArray();
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
        writer.WriteEndArray();
        return GzipUtil.Compress(writer.Encode());
    }
}
