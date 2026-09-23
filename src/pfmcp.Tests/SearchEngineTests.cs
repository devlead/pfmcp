namespace Pfmcp.Tests;

public sealed class SearchEngineTests
{
    [Fact]
    public async Task Searches_fixture_for_cake()
    {
        var directory = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-search-" + Guid.NewGuid().ToString("N")));
        var catalog = CreateCatalog();
        await catalog.InitializeAsync(new IndexSettings { Indexes = [directory] }, TestContext.Current.CancellationToken);

        var hits = await SearchEngine.SearchAsync(catalog.Bundles[0], "cake", null, 8, TestContext.Current.CancellationToken);

        await Verify(hits.Select(h => new { h.Url, h.Title, h.Excerpt, h.PageId, h.Score }));
    }

    [Fact]
    public async Task Searches_fixture_for_stemmed_cakes()
    {
        var directory = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-cakes-" + Guid.NewGuid().ToString("N")));
        var catalog = CreateCatalog();
        await catalog.InitializeAsync(new IndexSettings { Indexes = [directory] }, TestContext.Current.CancellationToken);

        var hits = await SearchEngine.SearchAsync(catalog.Bundles[0], "cakes", null, 8, TestContext.Current.CancellationToken);

        await Verify(hits.Select(h => new { h.Url, h.Title, h.Excerpt, h.PageId, h.Score }));
    }

    [Fact]
    public async Task Get_page_returns_fragment_content()
    {
        var directory = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-page-" + Guid.NewGuid().ToString("N")));
        var catalog = CreateCatalog();
        await catalog.InitializeAsync(new IndexSettings { Indexes = ["docs=" + directory] }, TestContext.Current.CancellationToken);

        var page = await SearchEngine.GetPageAsync(catalog.Bundles[0], "/cake/", null, TestContext.Current.CancellationToken);

        await Verify(page);
    }

    [Fact]
    public async Task Loads_two_named_indexes_and_resolves()
    {
        var docs = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-docs-" + Guid.NewGuid().ToString("N")));
        var blog = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-blog-" + Guid.NewGuid().ToString("N")));
        var catalog = CreateCatalog();
        await catalog.InitializeAsync(
            new IndexSettings { Indexes = ["docs=" + docs, "blog=" + blog] },
            TestContext.Current.CancellationToken);

        var hits = new List<SearchHit>();
        foreach (var bundle in catalog.Resolve("docs"))
        {
            hits.AddRange(await SearchEngine.SearchAsync(bundle, "cake", null, 8, TestContext.Current.CancellationToken));
        }

        await Verify(new
        {
            Names = catalog.Summaries().Select(s => s.Name),
            DocsCount = catalog.Resolve("docs").Count(),
            AllCount = catalog.Resolve(null).Count(),
            Hits = hits.Select(h => new { h.Url, h.Title, h.PageId, h.Index, h.Score })
        });
    }

    [Fact]
    public async Task Filters_restrict_results()
    {
        var directory = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-filter-" + Guid.NewGuid().ToString("N")));
        var catalog = CreateCatalog();
        await catalog.InitializeAsync(new IndexSettings { Indexes = [directory] }, TestContext.Current.CancellationToken);

        var hits = await SearchEngine.SearchAsync(
            catalog.Bundles[0],
            "tool",
            new Dictionary<string, string> { ["tag"] = "cloud" },
            8,
            TestContext.Current.CancellationToken);

        await Verify(hits.Select(h => new { h.Url, h.Title, h.Excerpt, h.PageId, h.Score }));
    }

    private static IndexCatalog CreateCatalog()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(IndexCatalog.HttpClientName);
        services.AddSingleton<IndexCatalog>();
        return services.BuildServiceProvider().GetRequiredService<IndexCatalog>();
    }
}
