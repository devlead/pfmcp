using System.Text.Json;

namespace Pfmcp.Tests;

public sealed class SearchParityTests
{
    [Theory]
    [InlineData("cake")]
    [InlineData("cakes")]
    [InlineData("Cake.")]
    public async Task Fixture_goldens_same_page_ids_and_order(string query)
    {
        var directory = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-parity-" + Guid.NewGuid().ToString("N")));
        var catalog = CreateCatalog();
        await catalog.InitializeAsync(new IndexSettings { Indexes = [directory] }, TestContext.Current.CancellationToken);

        var hits = await SearchEngine.SearchAsync(catalog.Bundles[0], query, null, 8, TestContext.Current.CancellationToken);

        await Verify(hits.Select(h => new { h.PageId, h.Url, h.Title, h.Score }));
    }

    [Fact]
    public async Task Fixture_title_boost_is_additive_not_multiplied()
    {
        var directory = FixtureBundle.Write(Path.Combine(Path.GetTempPath(), "pfmcp-title-" + Guid.NewGuid().ToString("N")));
        var catalog = CreateCatalog();
        await catalog.InitializeAsync(new IndexSettings { Indexes = [directory] }, TestContext.Current.CancellationToken);

        var hits = await SearchEngine.SearchAsync(catalog.Bundles[0], "cake", null, 8, TestContext.Current.CancellationToken);
        Assert.Single(hits);

        var ranking = PagefindRanking.DefaultWeights;
        var lengthBonus = PagefindRanking.WordLengthBonus(PagefindRanking.LengthDifferential(4, 4), ranking.TermSimilarity);
        var body = PagefindRanking.Bm25Score(25f / 24f, 12, 11, 2, 1, lengthBonus, ranking);
        var idf = PagefindRanking.CalculateIdf(2, 1);
        var expected = body * PagefindRanking.DiacriticBonus("cake", "cake", ranking.DiacriticSimilarity)
                       + (5f * idf * 1f * 1f);

        Assert.True(RelativeError(hits[0].Score, expected) <= 1e-5f, $"score {hits[0].Score} vs {expected}");
    }

    [Fact]
    public void Ranking_uses_tf_over_24_and_length_plus_one()
    {
        Assert.Equal(1, PagefindRanking.LengthDifferential(4, 4));
        Assert.Equal(3, PagefindRanking.LengthDifferential(6, 4));
        var ranking = PagefindRanking.DefaultWeights;
        var with24 = PagefindRanking.Bm25Score(24f / 24f, 100, 100, 10, 2, 1, ranking);
        var with25 = PagefindRanking.Bm25Score(25f / 25f, 100, 100, 10, 2, 1, ranking);
        Assert.Equal(with25, with24);
        Assert.NotEqual(1f, PagefindRanking.WordLengthBonus(1, 1f));
    }

    [Fact]
    public Task Query_pipeline_stems_like_pagefind()
    {
        var terms = QueryPipeline.Analyze("Cakes. rebasing", [], EnglishStemmer.Instance);
        return Verify(new
        {
            Original = terms.Select(t => t.Original),
            Stems = terms.Select(t => t.Stem),
            IndexKeys = QueryPipeline.IndexKeys(terms)
        });
    }

    [Fact]
    public Task Word_dictionary_prefix_and_longest_inverse()
    {
        var dict = new WordDictionary();
        dict.Add(new PackedWord { Word = "cake", Pages = [] });
        dict.Add(new PackedWord { Word = "rebas", Pages = [] });

        return Verify(new
        {
            PrefixCak = dict.FindWordExtensions("cak").Select(x => x.Key),
            InverseRebasing = dict.FindWordExtensions("rebasing").Select(x => x.Key)
        });
    }

    [Fact]
    [Trait(LiveIndexTests.TraitName, "true")]
    public async Task Live_goldens_same_ids_order_and_relative_scores()
    {
        var catalog = await TryLoadLiveAsync();
        if (catalog is null)
        {
            Assert.Skip("Live Pagefind index at https://www.devlead.se/pagefind/ is unreachable.");
        }

        var goldens = ReadLiveGoldens();
        if (goldens.Count == 0)
        {
            var cake = await SearchEngine.SearchAsync(catalog.Bundles[0], "cake", null, 8, TestContext.Current.CancellationToken);
            var cakes = await SearchEngine.SearchAsync(catalog.Bundles[0], "cakes", null, 8, TestContext.Current.CancellationToken);
            Assert.NotEmpty(cake);
            Assert.NotEmpty(cakes);
            Assert.Equal(cake.Select(h => h.PageId).Take(3), cakes.Select(h => h.PageId).Take(3));
            return;
        }

        foreach (var golden in goldens)
        {
            var hits = await SearchEngine.SearchAsync(
                catalog.Bundles[0],
                golden.Query,
                null,
                Math.Max(golden.PageIds.Length, 1),
                TestContext.Current.CancellationToken);

            Assert.Equal(
                golden.PageIds.ToHashSet(StringComparer.Ordinal),
                hits.Select(h => h.PageId).ToHashSet(StringComparer.Ordinal));
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static float RelativeError(float actual, float expected)
    {
        if (expected == 0)
        {
            return MathF.Abs(actual);
        }

        return MathF.Abs(actual - expected) / MathF.Abs(expected);
    }

    private static List<LiveGolden> ReadLiveGoldens()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "testdata", "oracle", "live");
        if (!Directory.Exists(dir))
        {
            return [];
        }

        return Directory.GetFiles(dir, "*.json")
            .Where(p => !p.EndsWith("queries.json", StringComparison.OrdinalIgnoreCase)
                        && !p.EndsWith("tool.json", StringComparison.OrdinalIgnoreCase))
            .Select(p => JsonSerializer.Deserialize<LiveGolden>(File.ReadAllText(p), JsonOptions))
            .Where(g => g is { PageIds.Length: > 0 })
            .Cast<LiveGolden>()
            .ToList();
    }

    private static IndexCatalog CreateCatalog()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(IndexCatalog.HttpClientName);
        services.AddSingleton<IndexCatalog>();
        return services.BuildServiceProvider().GetRequiredService<IndexCatalog>();
    }

    private static async Task<IndexCatalog?> TryLoadLiveAsync()
    {
        try
        {
            var catalog = CreateCatalog();
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

    private sealed class LiveGolden
    {
        public string Query { get; set; } = "";
        public string[] PageIds { get; set; } = [];
        public float[]? Scores { get; set; }
    }
}
