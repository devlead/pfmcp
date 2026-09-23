namespace Pfmcp.Services.Pagefind;

internal static partial class BundleLocator
{
    public static IndexSpec ParseSpec(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Index value is required.", nameof(value));
        }

        value = value.Trim();
        var eq = value.IndexOf('=');
        if (eq > 0
            && !value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            && !Path.IsPathRooted(value))
        {
            var name = value[..eq].Trim();
            var source = value[(eq + 1)..].Trim();
            if (name.Length > 0 && source.Length > 0 && !name.Contains('/') && !name.Contains('\\'))
            {
                return new IndexSpec(name, Normalize(source));
            }
        }

        var normalized = Normalize(value);
        return new IndexSpec(DefaultName(normalized), normalized);
    }

    public static IReadOnlyList<IndexSpec> ParseSpecs(IEnumerable<string>? values, string? environment)
    {
        var specs = new List<IndexSpec>();
        if (values is not null)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    specs.Add(ParseSpec(value));
                }
            }
        }

        if (specs.Count == 0 && !string.IsNullOrWhiteSpace(environment))
        {
            foreach (var part in environment.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                specs.Add(ParseSpec(part));
            }
        }

        return specs;
    }

    private static string DefaultName(string source)
    {
        if (IsHttp(source))
        {
            var uri = new Uri(source);
            var last = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()
                       ?? uri.Host;
            return last.Equals("pagefind", StringComparison.OrdinalIgnoreCase) ? uri.Host : last;
        }

        return new DirectoryInfo(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).Name;
    }
}
