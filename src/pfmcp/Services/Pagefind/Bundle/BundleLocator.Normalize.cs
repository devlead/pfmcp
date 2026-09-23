namespace Pfmcp.Services.Pagefind;

internal static partial class BundleLocator
{
    public static string Normalize(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Index source is required.", nameof(source));
        }

        source = source.Trim();

        if (Uri.TryCreate(source, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return NormalizeHttp(uri);
        }

        return NormalizePath(source);
    }

    public static bool IsHttp(string source)
        => Uri.TryCreate(source, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static string NormalizeHttp(Uri uri)
    {
        var builder = new UriBuilder(uri) { Query = string.Empty, Fragment = string.Empty };
        var path = builder.Path;
        if (path.EndsWith(EntryFileName, StringComparison.OrdinalIgnoreCase))
        {
            path = path[..^EntryFileName.Length];
        }

        if (!path.EndsWith('/'))
        {
            path += "/";
        }

        builder.Path = path;
        return builder.Uri.ToString();
    }

    private static string NormalizePath(string source)
    {
        var full = Path.GetFullPath(source);
        if (File.Exists(full) && full.EndsWith(EntryFileName, StringComparison.OrdinalIgnoreCase))
        {
            return EnsureTrailingSlash(Path.GetDirectoryName(full)!);
        }

        if (Directory.Exists(full))
        {
            var entry = Path.Combine(full, EntryFileName);
            if (File.Exists(entry))
            {
                return EnsureTrailingSlash(full);
            }

            var nested = Path.Combine(full, "pagefind");
            if (File.Exists(Path.Combine(nested, EntryFileName)))
            {
                return EnsureTrailingSlash(nested);
            }
        }

        return EnsureTrailingSlash(full);
    }

    private static string EnsureTrailingSlash(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}
