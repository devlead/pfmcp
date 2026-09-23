namespace Pfmcp.Commands;

public sealed class SearchSettings : IndexSettings
{
    [CommandArgument(0, "<QUERY>")]
    [Description("Search query.")]
    public string Query { get; init; } = "";

    [CommandOption("-l|--limit <LIMIT>")]
    [Description("Maximum number of results (default: 8).")]
    public int Limit { get; init; } = 8;

    [CommandOption("--name <NAME>")]
    [Description("Limit search to a named index.")]
    public string? Name { get; init; }
}
