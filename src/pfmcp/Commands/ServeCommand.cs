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
                    """
                    Search static-site Pagefind indexes (site search, not generic web search).

                    Tools: pfs (search), pfli (indexes/source/languages/page counts), pflf (filter keys/values; skip when filterCount is 0), pfget (full page by url or pageId). Pass index on pfget and pflf only when more than one index is configured.

                    pfli source is the Pagefind index root (often https://host/pagefind/), not the site origin. Site base URL = origin of source with trailing /pagefind stripped. pfs url is usually site-relative; full URL = site base + path (do not prefix /pagefind/). If url is already absolute, use it as-is.

                    When the user searches: call pfs with that query. Do not pfget unless they ask for content, a summary, or a quote. If the site base URL is unknown, call pfli once and derive it. Present hits as a markdown table, highest score first: Score | Title as [title](full-url); optional Excerpt column. Never show a bare relative path as the only URL. Do not dump raw tool JSON when a table will do.

                    pfget only for a few promising hits, or when asked to read a specific result. Prefer url; pageId is a fallback.

                    filters is a JSON object of key/value pairs (AND). Discover keys with pflf first. Omit filters when unused.

                    Do not treat Pagefind source as a page URL, call pfli on every search after the origin is known, or fetch every hit with pfget.
                    """;
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync(cancellationToken).ConfigureAwait(false);
        return 0;
    }
}
