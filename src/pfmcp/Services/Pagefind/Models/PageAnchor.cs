namespace Pfmcp.Services.Pagefind.Models;

internal sealed class PageAnchor
{
    [JsonPropertyName("element")]
    public string Element { get; set; } = "";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("location")]
    public uint Location { get; set; }
}
