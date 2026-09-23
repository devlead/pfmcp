namespace Pfmcp.Services.Pagefind;

internal static partial class PagefindCodec
{
    public static WordIndex DecodeWordIndex(byte[] bytes)
    {
        var payload = GzipUtil.DecompressIfNeeded(bytes);
        var reader = new CborReader(payload, CborConformanceMode.Lax);
        CborUtil.ReadArrayLength(reader);

        var words = new List<PackedWord>();
        var wordCount = CborUtil.ReadArrayLength(reader);
        for (var i = 0; i < wordCount; i++)
        {
            var wordArrLen = CborUtil.ReadArrayLength(reader);
            var word = reader.ReadTextString();
            var pages = DecodePages(reader);
            var variants = new List<PackedVariant>();
            if (wordArrLen >= 3)
            {
                var variantCount = CborUtil.ReadArrayLength(reader);
                for (var v = 0; v < variantCount; v++)
                {
                    CborUtil.ReadArrayLength(reader);
                    variants.Add(new PackedVariant
                    {
                        Form = reader.ReadTextString(),
                        Pages = DecodePages(reader)
                    });
                    reader.ReadEndArray();
                }

                reader.ReadEndArray();
            }

            reader.ReadEndArray();
            words.Add(new PackedWord
            {
                Word = word,
                Pages = pages,
                AdditionalVariants = variants
            });
        }

        reader.ReadEndArray();
        return new WordIndex { Words = words };
    }

    public static byte[] EncodeWordIndex(WordIndex index)
    {
        var writer = new CborWriter();
        writer.WriteStartArray(1);
        writer.WriteStartArray(index.Words.Count);
        foreach (var word in index.Words)
        {
            var hasVariants = word.AdditionalVariants.Count > 0;
            writer.WriteStartArray(hasVariants ? 3 : 2);
            writer.WriteTextString(word.Word);
            EncodePages(writer, word.Pages);
            if (hasVariants)
            {
                writer.WriteStartArray(word.AdditionalVariants.Count);
                foreach (var variant in word.AdditionalVariants)
                {
                    writer.WriteStartArray(2);
                    writer.WriteTextString(variant.Form);
                    EncodePages(writer, variant.Pages);
                    writer.WriteEndArray();
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
        writer.WriteEndArray();
        return GzipUtil.Compress(writer.Encode());
    }
}
