namespace Pfmcp.Services.Pagefind;

internal static partial class PagefindCodec
{
    public static MetaIndex DecodeMeta(byte[] bytes)
    {
        var payload = GzipUtil.DecompressIfNeeded(bytes);
        var reader = new CborReader(payload, CborConformanceMode.Lax);

        var outer = CborUtil.ReadArrayLength(reader);
        var version = reader.ReadTextString();

        var pages = new List<MetaPage>();
        var pageCount = CborUtil.ReadArrayLength(reader);
        for (var i = 0; i < pageCount; i++)
        {
            CborUtil.ReadArrayLength(reader);
            pages.Add(new MetaPage
            {
                Hash = reader.ReadTextString(),
                WordCount = CborUtil.ReadUInt32(reader)
            });
            reader.ReadEndArray();
        }

        reader.ReadEndArray();

        var chunks = new List<MetaChunk>();
        var chunkCount = CborUtil.ReadArrayLength(reader);
        for (var i = 0; i < chunkCount; i++)
        {
            CborUtil.ReadArrayLength(reader);
            chunks.Add(new MetaChunk
            {
                From = reader.ReadTextString(),
                To = reader.ReadTextString(),
                Hash = reader.ReadTextString()
            });
            reader.ReadEndArray();
        }

        reader.ReadEndArray();

        var filters = new List<MetaFilter>();
        var filterCount = CborUtil.ReadArrayLength(reader);
        for (var i = 0; i < filterCount; i++)
        {
            CborUtil.ReadArrayLength(reader);
            filters.Add(new MetaFilter
            {
                Filter = reader.ReadTextString(),
                Hash = reader.ReadTextString()
            });
            reader.ReadEndArray();
        }

        reader.ReadEndArray();

        var sorts = new List<MetaSort>();
        var sortCount = CborUtil.ReadArrayLength(reader);
        for (var i = 0; i < sortCount; i++)
        {
            CborUtil.ReadArrayLength(reader);
            var key = reader.ReadTextString();
            var pageNums = new List<uint>();
            var n = CborUtil.ReadArrayLength(reader);
            for (var p = 0; p < n; p++)
            {
                pageNums.Add(CborUtil.ReadUInt32(reader));
            }

            reader.ReadEndArray();
            sorts.Add(new MetaSort { Sort = key, Pages = pageNums });
            reader.ReadEndArray();
        }

        reader.ReadEndArray();

        var metaFields = new List<string>();
        if (outer >= 6)
        {
            var fieldCount = CborUtil.ReadArrayLength(reader);
            for (var i = 0; i < fieldCount; i++)
            {
                metaFields.Add(reader.ReadTextString());
            }

            reader.ReadEndArray();
        }

        reader.ReadEndArray();

        return new MetaIndex
        {
            Version = version,
            Pages = pages,
            IndexChunks = chunks,
            Filters = filters,
            Sorts = sorts,
            MetaFields = metaFields
        };
    }

    public static byte[] EncodeMeta(MetaIndex meta)
    {
        var writer = new CborWriter();
        var outer = meta.MetaFields.Count > 0 ? 6 : 5;
        writer.WriteStartArray(outer);
        writer.WriteTextString(meta.Version);

        writer.WriteStartArray(meta.Pages.Count);
        foreach (var page in meta.Pages)
        {
            writer.WriteStartArray(2);
            writer.WriteTextString(page.Hash);
            writer.WriteUInt32(page.WordCount);
            writer.WriteEndArray();
        }

        writer.WriteEndArray();

        writer.WriteStartArray(meta.IndexChunks.Count);
        foreach (var chunk in meta.IndexChunks)
        {
            writer.WriteStartArray(3);
            writer.WriteTextString(chunk.From);
            writer.WriteTextString(chunk.To);
            writer.WriteTextString(chunk.Hash);
            writer.WriteEndArray();
        }

        writer.WriteEndArray();

        writer.WriteStartArray(meta.Filters.Count);
        foreach (var filter in meta.Filters)
        {
            writer.WriteStartArray(2);
            writer.WriteTextString(filter.Filter);
            writer.WriteTextString(filter.Hash);
            writer.WriteEndArray();
        }

        writer.WriteEndArray();

        writer.WriteStartArray(meta.Sorts.Count);
        foreach (var sort in meta.Sorts)
        {
            writer.WriteStartArray(2);
            writer.WriteTextString(sort.Sort);
            writer.WriteStartArray(sort.Pages.Count);
            foreach (var page in sort.Pages)
            {
                writer.WriteUInt32(page);
            }

            writer.WriteEndArray();
            writer.WriteEndArray();
        }

        writer.WriteEndArray();

        if (meta.MetaFields.Count > 0)
        {
            writer.WriteStartArray(meta.MetaFields.Count);
            foreach (var field in meta.MetaFields)
            {
                writer.WriteTextString(field);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
        return GzipUtil.Compress(writer.Encode());
    }
}
