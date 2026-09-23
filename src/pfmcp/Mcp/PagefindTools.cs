namespace Pfmcp.Mcp;

[McpServerToolType]
internal sealed partial class PagefindTools(IndexCatalog catalog)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}
