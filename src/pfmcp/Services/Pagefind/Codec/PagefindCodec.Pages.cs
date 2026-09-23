namespace Pfmcp.Services.Pagefind;

internal static partial class PagefindCodec
{
    private static List<PackedPage> DecodePages(CborReader reader)
    {
        var pages = new List<PackedPage>();
        var pageCount = CborUtil.ReadArrayLength(reader);
        uint cumulative = 0;
        for (var i = 0; i < pageCount; i++)
        {
            var pageArrLen = CborUtil.ReadArrayLength(reader);
            cumulative += CborUtil.ReadUInt32(reader);

            var locs = new List<(byte Weight, uint Location)>();
            var locCount = CborUtil.ReadArrayLength(reader);
            byte weight = 25;
            uint lastPosition = 0;
            for (var l = 0; l < locCount; l++)
            {
                var loc = CborUtil.ReadInt(reader);
                if (loc < 0)
                {
                    var absWeight = (loc + 1) * -1;
                    weight = absWeight > 255 ? (byte)255 : (byte)absWeight;
                    lastPosition = 0;
                }
                else
                {
                    lastPosition += (uint)loc;
                    locs.Add((weight, lastPosition));
                }
            }

            reader.ReadEndArray();

            var metaLocs = new List<(ushort FieldId, uint Location)>();
            if (pageArrLen >= 3)
            {
                var metaCount = CborUtil.ReadArrayLength(reader);
                ushort currentField = 0;
                uint lastMeta = 0;
                for (var m = 0; m < metaCount; m++)
                {
                    var loc = CborUtil.ReadInt(reader);
                    if (loc < 0)
                    {
                        currentField = (ushort)(-loc - 1);
                        lastMeta = 0;
                    }
                    else
                    {
                        lastMeta += (uint)loc;
                        metaLocs.Add((currentField, lastMeta));
                    }
                }

                reader.ReadEndArray();
            }

            reader.ReadEndArray();
            pages.Add(new PackedPage
            {
                PageNumber = cumulative,
                Locs = locs,
                MetaLocs = metaLocs
            });
        }

        reader.ReadEndArray();
        return pages;
    }

    private static void EncodePages(CborWriter writer, List<PackedPage> pages)
    {
        writer.WriteStartArray(pages.Count);
        uint previousPage = 0;
        foreach (var page in pages)
        {
            var hasMeta = page.MetaLocs.Count > 0;
            writer.WriteStartArray(hasMeta ? 3 : 2);
            writer.WriteUInt32(page.PageNumber - previousPage);
            previousPage = page.PageNumber;

            writer.WriteStartArray(page.Locs.Count + CountWeightMarkers(page.Locs));
            byte currentWeight = 25;
            uint lastPos = 0;
            foreach (var (weight, location) in page.Locs)
            {
                if (weight != currentWeight)
                {
                    writer.WriteInt64(-(weight + 1));
                    currentWeight = weight;
                    lastPos = 0;
                }

                writer.WriteInt64(location - lastPos);
                lastPos = location;
            }

            writer.WriteEndArray();

            if (hasMeta)
            {
                writer.WriteStartArray(page.MetaLocs.Count + CountFieldMarkers(page.MetaLocs));
                ushort field = 0;
                uint lastMeta = 0;
                var wroteField = false;
                foreach (var (fieldId, location) in page.MetaLocs)
                {
                    if (!wroteField || fieldId != field)
                    {
                        writer.WriteInt64(-(fieldId + 1));
                        field = fieldId;
                        lastMeta = 0;
                        wroteField = true;
                    }

                    writer.WriteInt64(location - lastMeta);
                    lastMeta = location;
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }

    private static int CountWeightMarkers(List<(byte Weight, uint Location)> locs)
    {
        byte current = 25;
        var markers = 0;
        foreach (var (weight, _) in locs)
        {
            if (weight != current)
            {
                markers++;
                current = weight;
            }
        }

        return markers;
    }

    private static int CountFieldMarkers(List<(ushort FieldId, uint Location)> locs)
    {
        var seen = new HashSet<ushort>();
        foreach (var (fieldId, _) in locs)
        {
            seen.Add(fieldId);
        }

        return seen.Count;
    }
}
