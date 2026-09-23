namespace Pfmcp.Tests;

public sealed class LiveIndexTests
{
    public const string TraitName = "Network";

    [Fact]
    [Trait(TraitName, "true")]
    public async Task Inspects_devlead_se_index()
    {
        var catalog = await TryLoadLiveAsync();
        if (catalog is null)
        {
            Assert.Skip("Live Pagefind index at https://www.devlead.se/pagefind/ is unreachable.");
        }

        var summary = catalog.Summaries().Single();
        Assert.Equal("1.5.2", summary.Version);
        Assert.Contains(summary.Languages, l => l.Language == "en" && l.PageCount == 34);
    }

    [Fact]
    [Trait(TraitName, "true")]
    public async Task Searches_devlead_se_for_cake()
    {
        var catalog = await TryLoadLiveAsync();
        if (catalog is null)
        {
            Assert.Skip("Live Pagefind index at https://www.devlead.se/pagefind/ is unreachable.");
        }

        var hits = await SearchEngine.SearchAsync(catalog.Bundles[0], "cake", null, 8, TestContext.Current.CancellationToken);
        Assert.NotEmpty(hits);
        Assert.Contains(hits, h => h.Title.Contains("cake", StringComparison.OrdinalIgnoreCase)
                                   || h.Excerpt.Contains("cake", StringComparison.OrdinalIgnoreCase)
                                   || h.Url.Contains("cake", StringComparison.OrdinalIgnoreCase));

        var stemmed = await SearchEngine.SearchAsync(catalog.Bundles[0], "cakes", null, 8, TestContext.Current.CancellationToken);
        Assert.NotEmpty(stemmed);
        Assert.NotEmpty(hits.Select(h => h.PageId).Intersect(stemmed.Select(h => h.PageId)));
    }

    private static async Task<IndexCatalog?> TryLoadLiveAsync()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddHttpClient(IndexCatalog.HttpClientName, client => client.Timeout = TimeSpan.FromSeconds(30));
            services.AddSingleton<IndexCatalog>();
            var catalog = services.BuildServiceProvider().GetRequiredService<IndexCatalog>();
            await catalog.InitializeAsync(
                new IndexSettings { Indexes = [LiveIndex.DevleadEntry] },
                TestContext.Current.CancellationToken);
            return catalog;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }
}
