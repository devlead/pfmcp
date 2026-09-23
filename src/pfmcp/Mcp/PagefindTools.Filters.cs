namespace Pfmcp.Mcp;

internal sealed partial class PagefindTools
{
    [McpServerTool(Name = "list_filters"), Description("List filter keys and values (with counts) for an index.")]
    public async Task<string> ListFilters(
        [Description("Optional index name. Required when more than one index is configured.")] string? index = null,
        CancellationToken cancellationToken = default)
    {
        var bundles = catalog.Resolve(index).ToList();
        if (bundles.Count == 0)
        {
            return JsonSerializer.Serialize(new { error = "Index not found." }, JsonOptions);
        }

        var result = new Dictionary<string, object>();
        foreach (var bundle in bundles)
        {
            result[bundle.Name] = await SearchEngine.ListFiltersAsync(bundle, cancellationToken).ConfigureAwait(false);
        }

        return JsonSerializer.Serialize(result, JsonOptions);
    }
}
