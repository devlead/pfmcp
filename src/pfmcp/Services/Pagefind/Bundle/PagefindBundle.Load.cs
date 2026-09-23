namespace Pfmcp.Services.Pagefind;

internal sealed partial class PagefindBundle
{
    public static async Task<PagefindBundle> LoadAsync(
        string name,
        IIndexStore store,
        CancellationToken cancellationToken)
    {
        var entryBytes = await store.ReadAsync(BundleLocator.EntryFileName, cancellationToken).ConfigureAwait(false);
        var entry = PagefindCodec.DecodeEntry(entryBytes);
        if (entry.Languages.Count == 0)
        {
            throw new InvalidDataException("Pagefind entry contains no languages.");
        }

        var language = entry.Languages.ContainsKey("en")
            ? "en"
            : entry.Languages.Keys.First();
        var lang = entry.Languages[language];
        var metaBytes = await store.ReadAsync($"pagefind.{lang.Hash}.pf_meta", cancellationToken).ConfigureAwait(false);
        var meta = PagefindCodec.DecodeMeta(metaBytes);
        return new PagefindBundle(name, store, entry, language, meta);
    }
}
