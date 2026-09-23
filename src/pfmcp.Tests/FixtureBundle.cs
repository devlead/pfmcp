namespace Pfmcp.Tests;

internal static class FixtureBundle
{
    public static string CommittedDirectory
    {
        get
        {
            var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "testdata", "pagefind"));
            if (!File.Exists(Path.Combine(directory, "pagefind-entry.json")))
            {
                Write(directory);
            }

            return directory;
        }
    }

    public static string Write(string directory)
    {
        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(Path.Combine(directory, "index"));
        Directory.CreateDirectory(Path.Combine(directory, "fragment"));
        Directory.CreateDirectory(Path.Combine(directory, "filter"));

        var entry = """
            {"version":"1.5.2","languages":{"en":{"hash":"en_fixture","wasm":"en","page_count":2}},"include_characters":["_"]}
            """;
        File.WriteAllText(Path.Combine(directory, "pagefind-entry.json"), entry);

        var meta = new MetaIndex
        {
            Version = "1.5.2",
            Pages =
            [
                new MetaPage { Hash = "en_page1", WordCount = 12 },
                new MetaPage { Hash = "en_page2", WordCount = 10 }
            ],
            IndexChunks =
            [
                new MetaChunk { From = "a", To = "z", Hash = "en_chunk" }
            ],
            Filters =
            [
                new MetaFilter { Filter = "tag", Hash = "en_tag" }
            ],
            MetaFields = ["title"]
        };
        File.WriteAllBytes(Path.Combine(directory, "pagefind.en_fixture.pf_meta"), PagefindCodec.EncodeMeta(meta));

        var words = new WordIndex
        {
            Words =
            [
                new PackedWord
                {
                    Word = "cake",
                    Pages =
                    [
                        new PackedPage
                        {
                            PageNumber = 0,
                            Locs = [(25, 3)],
                            MetaLocs = [(0, 0)]
                        }
                    ]
                },
                new PackedWord
                {
                    Word = "azure",
                    Pages =
                    [
                        new PackedPage
                        {
                            PageNumber = 1,
                            Locs = [(25, 2)]
                        }
                    ]
                },
                new PackedWord
                {
                    Word = "tool",
                    Pages =
                    [
                        new PackedPage { PageNumber = 0, Locs = [(25, 6)] },
                        new PackedPage { PageNumber = 1, Locs = [(25, 5)] }
                    ]
                }
            ]
        };
        File.WriteAllBytes(Path.Combine(directory, "index", "en_chunk.pf_index"), PagefindCodec.EncodeWordIndex(words));

        File.WriteAllBytes(
            Path.Combine(directory, "fragment", "en_page1.pf_fragment"),
            PagefindCodec.EncodeFragment(new PageFragment
            {
                Url = "/cake/",
                Content = "Hello from the cake build tool documentation page.",
                WordCount = 8,
                Meta = new Dictionary<string, string> { ["title"] = "Cake docs" },
                Anchors = [new PageAnchor { Element = "h2", Id = "intro", Text = "Intro", Location = 0 }]
            }));

        File.WriteAllBytes(
            Path.Combine(directory, "fragment", "en_page2.pf_fragment"),
            PagefindCodec.EncodeFragment(new PageFragment
            {
                Url = "/azure/",
                Content = "Using azure identity for the tool.",
                WordCount = 6,
                Meta = new Dictionary<string, string> { ["title"] = "Azure identity" }
            }));

        File.WriteAllBytes(
            Path.Combine(directory, "filter", "en_tag.pf_filter"),
            PagefindCodec.EncodeFilterIndex(new FilterIndex
            {
                Filter = "tag",
                Values = new Dictionary<string, List<uint>>
                {
                    ["docs"] = [0],
                    ["cloud"] = [1]
                }
            }));

        return directory;
    }
}
