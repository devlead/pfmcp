namespace Pfmcp.Commands;

public sealed class InspectCommand(IndexCatalog catalog) : AsyncCommand<IndexSettings>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected override async Task<int> ExecuteAsync(CommandContext context, IndexSettings settings, CancellationToken cancellationToken)
    {
        await catalog.InitializeAsync(settings, cancellationToken).ConfigureAwait(false);
        AnsiConsole.WriteLine(JsonSerializer.Serialize(catalog.Summaries(), JsonOptions));
        return 0;
    }
}
