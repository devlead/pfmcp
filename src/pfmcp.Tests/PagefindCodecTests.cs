namespace Pfmcp.Tests;

public sealed class PagefindCodecTests
{
    [Fact]
    public Task Roundtrips_meta_index_and_word_index()
    {
        var directory = FixtureBundle.CommittedDirectory;

        var entry = PagefindCodec.DecodeEntry(File.ReadAllBytes(Path.Combine(directory, "pagefind-entry.json")));
        var meta = PagefindCodec.DecodeMeta(File.ReadAllBytes(Path.Combine(directory, "pagefind.en_fixture.pf_meta")));
        var words = PagefindCodec.DecodeWordIndex(File.ReadAllBytes(Path.Combine(directory, "index", "en_chunk.pf_index")));
        var fragment = PagefindCodec.DecodeFragment(File.ReadAllBytes(Path.Combine(directory, "fragment", "en_page1.pf_fragment")));

        return Verify(new { entry, meta, words, fragment });
    }
}
