namespace Pfmcp.Services.Pagefind;

internal static class ExcerptBuilder
{
    public static string Build(string content, IReadOnlyList<uint> locations, int window = 12)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var words = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return string.Empty;
        }

        var center = locations.Count > 0
            ? (int)Math.Min(locations[0], (uint)(words.Length - 1))
            : 0;
        var start = Math.Max(0, center - window);
        var end = Math.Min(words.Length, center + window + 1);
        var excerpt = string.Join(' ', words[start..end]);
        if (start > 0)
        {
            excerpt = "…" + excerpt;
        }

        if (end < words.Length)
        {
            excerpt += "…";
        }

        return excerpt;
    }
}
