namespace Pfmcp.Commands;

public sealed class SearchCommand(IndexCatalog catalog) : AsyncCommand<SearchSettings>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected override async Task<int> ExecuteAsync(CommandContext context, SearchSettings settings, CancellationToken cancellationToken)
    {
        await catalog.InitializeAsync(settings, cancellationToken).ConfigureAwait(false);
        var hits = new List<SearchHit>();
        foreach (var bundle in catalog.Resolve(settings.Name))
        {
            hits.AddRange(await SearchEngine.SearchAsync(bundle, settings.Query, null, settings.Limit, cancellationToken).ConfigureAwait(false));
        }

        var results = hits
            .OrderByDescending(h => h.Score)
            .Take(Math.Max(1, settings.Limit))
            .ToList();

        AnsiConsole.WriteLine(JsonSerializer.Serialize(results, JsonOptions));
        return 0;
    }
}
