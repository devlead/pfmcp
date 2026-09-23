namespace Pfmcp.Mcp;

internal sealed partial class PagefindTools
{
    [McpServerTool(Name = "get_page"), Description("Load full page text from a Pagefind fragment by url or pageId. Pass index when multiple indexes are configured.")]
    public async Task<string> GetPage(
        [Description("Page URL from a search result.")] string? url = null,
        [Description("Page id (fragment hash) from a search result.")] string? pageId = null,
        [Description("Optional index name when multiple indexes are configured.")] string? index = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(pageId))
        {
            return JsonSerializer.Serialize(new { error = "Provide url or pageId." }, JsonOptions);
        }

        foreach (var bundle in catalog.Resolve(index))
        {
            var page = await SearchEngine.GetPageAsync(bundle, url, pageId, cancellationToken).ConfigureAwait(false);
            if (page is not null)
            {
                return JsonSerializer.Serialize(page, JsonOptions);
            }
        }

        return JsonSerializer.Serialize(new { error = "Page not found." }, JsonOptions);
    }
}
