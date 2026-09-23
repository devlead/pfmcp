namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PagefindEntryLanguage
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = "";

    [JsonPropertyName("wasm")]
    public string? Wasm { get; set; }

    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
}
