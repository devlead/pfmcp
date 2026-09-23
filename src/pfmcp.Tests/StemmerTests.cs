namespace Pfmcp.Tests;

public sealed class StemmerTests
{
    [Theory]
    [InlineData("configuration", "configur")]
    [InlineData("configuring", "configur")]
    [InlineData("cakes", "cake")]
    [InlineData("cake", "cake")]
    [InlineData("rebasing", "rebas")]
    [InlineData("rebase", "rebas")]
    [InlineData("tools", "tool")]
    [InlineData("running", "run")]
    [InlineData("lying", "lie")]
    [InlineData("dying", "die")]
    [InlineData("news", "news")]
    [InlineData("sky", "sky")]
    [InlineData("by", "by")]
    public void Snowball_english_known_vectors(string word, string stem)
    {
        Assert.Equal(stem, EnglishStemmer.Instance.Stem(word));
    }

    [Fact]
    public void Committed_stem_vectors_match()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "testdata", "stem-vectors.txt"));
        Assert.True(File.Exists(path), path);
        var failures = new List<string>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split('\t', 2);
            Assert.Equal(2, parts.Length);
            var actual = EnglishStemmer.Instance.Stem(parts[0]);
            if (!string.Equals(actual, parts[1], StringComparison.Ordinal))
            {
                failures.Add($"{parts[0]}: expected {parts[1]}, got {actual}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Factory_maps_en_wasm_to_english()
    {
        Assert.IsType<EnglishStemmer>(StemmerFactory.Create("en"));
        Assert.IsType<EnglishStemmer>(StemmerFactory.Create("en-US"));
        Assert.IsType<IdentityStemmer>(StemmerFactory.Create("sv"));
    }
}
