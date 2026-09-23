using System.IO.Compression;
using System.Text;

namespace Pfmcp.Services.Pagefind;

internal static class GzipUtil
{
    private static readonly byte[] Signature = Encoding.ASCII.GetBytes("pagefind_dcd");

    public static byte[] DecompressIfNeeded(byte[] data)
    {
        if (HasSignature(data))
        {
            return data[Signature.Length..];
        }

        if (data.Length >= 2 && data[0] == 0x1f && data[1] == 0x8b)
        {
            using var input = new MemoryStream(data);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            data = output.ToArray();
        }

        if (HasSignature(data))
        {
            return data[Signature.Length..];
        }

        return data;
    }

    public static byte[] Compress(byte[] data)
    {
        var framed = new byte[Signature.Length + data.Length];
        Buffer.BlockCopy(Signature, 0, framed, 0, Signature.Length);
        Buffer.BlockCopy(data, 0, framed, Signature.Length, data.Length);

        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            gzip.Write(framed);
        }

        return output.ToArray();
    }

    private static bool HasSignature(byte[] data)
        => data.Length >= Signature.Length && data.AsSpan(0, Signature.Length).SequenceEqual(Signature);
}
