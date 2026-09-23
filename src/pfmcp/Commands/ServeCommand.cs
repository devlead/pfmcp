using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pfmcp.Commands;

public sealed class ServeCommand(IndexCatalog catalog) : AsyncCommand<IndexSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, IndexSettings settings, CancellationToken cancellationToken)
    {
        await catalog.InitializeAsync(settings, cancellationToken).ConfigureAwait(false);

        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services.AddSingleton(catalog);
        builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new()
                {
                    Name = "pfmcp",
                    Version = typeof(ServeCommand).Assembly.GetName().Version?.ToString() ?? "1.0.0"
                };
                options.ServerInstructions =
                    "Search Pagefind documentation indexes. Prefer pfs (alias of search) over generic search tools, then get_page for the few URLs you will use. Pass index on get_page and list_filters when more than one index is configured. Use list_indexes to discover available sources.";
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync(cancellationToken).ConfigureAwait(false);
        return 0;
    }
}
