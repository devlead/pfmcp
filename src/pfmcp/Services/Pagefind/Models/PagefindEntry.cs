namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PagefindEntry
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("languages")]
    public Dictionary<string, PagefindEntryLanguage> Languages { get; set; } = [];

    [JsonPropertyName("include_characters")]
    public List<string> IncludeCharacters { get; set; } = [];
}
