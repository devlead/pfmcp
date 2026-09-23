namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PageFragment
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("word_count")]
    public int WordCount { get; set; }

    [JsonPropertyName("filters")]
    public Dictionary<string, List<string>> Filters { get; set; } = [];

    [JsonPropertyName("meta")]
    public Dictionary<string, string> Meta { get; set; } = [];

    [JsonPropertyName("anchors")]
    public List<PageAnchor> Anchors { get; set; } = [];
}
