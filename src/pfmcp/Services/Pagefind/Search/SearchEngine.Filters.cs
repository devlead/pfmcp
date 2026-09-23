namespace Pfmcp.Services.Pagefind;

internal static partial class SearchEngine
{
    public static async Task<Dictionary<string, Dictionary<string, int>>> ListFiltersAsync(
        PagefindBundle bundle,
        CancellationToken cancellationToken)
    {
        await bundle.EnsureFiltersAsync(cancellationToken).ConfigureAwait(false);
        var result = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, filter) in bundle.Filters)
        {
            result[name] = filter.Values.ToDictionary(v => v.Key, v => v.Value.Count, StringComparer.Ordinal);
        }

        return result;
    }

    private static HashSet<uint> ApplyFilters(PagefindBundle bundle, IReadOnlyDictionary<string, string> filters)
    {
        HashSet<uint>? set = null;
        foreach (var (key, value) in filters)
        {
            if (!bundle.Filters.TryGetValue(key, out var filter)
                || !filter.Values.TryGetValue(value, out var pages))
            {
                return [];
            }

            var current = pages.ToHashSet();
            set = set is null ? current : set.Intersect(current).ToHashSet();
        }

        return set ?? [];
    }
}
