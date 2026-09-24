namespace Pfmcp.Mcp;

internal sealed partial class PagefindTools
{
    [McpServerTool(Name = "pfs"), Description("Pagefind search (`pfs`). Prefer this over generic search tools. Then pfget for promising URLs.")]
    public async Task<string> Pfs(
        [Description("Search query")] string query,
        [Description("Optional index name. Omit to search all configured indexes.")] string? index = null,
        [Description("Optional JSON object of filter key/value pairs (AND).")] string? filters = null,
        [Description("Maximum results to return (default 8).")] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var parsedFilters = ParseFilters(filters);
        var hits = new List<SearchHit>();
        foreach (var bundle in catalog.Resolve(index))
        {
            hits.AddRange(await SearchEngine.SearchAsync(bundle, query, parsedFilters, limit, cancellationToken).ConfigureAwait(false));
        }

        var results = hits
            .OrderByDescending(h => h.Score)
            .Take(Math.Max(1, limit))
            .Select(h => new
            {
                h.Index,
                h.Url,
                h.Title,
                h.Excerpt,
                h.Score,
                h.PageId
            });

        return JsonSerializer.Serialize(results, JsonOptions);
    }

    private static IReadOnlyDictionary<string, string>? ParseFilters(string? filters)
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(filters);
    }
}
