namespace Pfmcp.Services.Pagefind;

internal static class CborUtil
{
    public static int ReadArrayLength(CborReader reader)
    {
        var length = reader.ReadStartArray();
        if (length is null)
        {
            throw new InvalidDataException("Expected a definite-length CBOR array.");
        }

        return checked((int)length.Value);
    }

    // Skips trailing items newer Pagefind versions may append, then closes the array.
    public static void ReadEndArray(CborReader reader)
    {
        while (reader.PeekState() != CborReaderState.EndArray)
        {
            reader.SkipValue();
        }

        reader.ReadEndArray();
    }

    public static uint ReadUInt32(CborReader reader)
        => checked((uint)reader.ReadUInt64());

    public static long ReadInt(CborReader reader)
        => reader.ReadInt64();
}
