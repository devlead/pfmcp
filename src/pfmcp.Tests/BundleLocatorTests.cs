namespace Pfmcp.Tests;

public sealed class BundleLocatorTests
{
    [Fact]
    public Task Normalizes_entry_json_http_url_to_bundle_dir()
        => Verify(BundleLocator.Normalize("https://www.devlead.se/pagefind/pagefind-entry.json"));

    [Fact]
    public Task Parses_named_index()
        => Verify(BundleLocator.ParseSpec("blog=https://www.devlead.se/pagefind/"));

    [Fact]
    public Task Parses_environment_list()
        => Verify(BundleLocator.ParseSpecs(null, "blog=https://www.devlead.se/pagefind/;https://example.com/pagefind/"));
}
