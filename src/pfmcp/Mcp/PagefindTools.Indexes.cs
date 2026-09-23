namespace Pfmcp.Mcp;

internal sealed partial class PagefindTools
{
    [McpServerTool(Name = "list_indexes"), Description("List configured Pagefind indexes, languages, and page counts.")]
    public string ListIndexes()
        => JsonSerializer.Serialize(catalog.Summaries(), JsonOptions);
}
