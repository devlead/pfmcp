namespace Pfmcp.Services.Pagefind;

internal sealed partial class PagefindBundle
{
    private readonly IIndexStore _store;
    private readonly Dictionary<string, WordIndex> _chunks = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FilterIndex> _filters = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PageFragment> _fragments = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _lock = new(1, 1);

    public PagefindBundle(string name, IIndexStore store, PagefindEntry entry, string language, MetaIndex meta)
    {
        Name = name;
        _store = store;
        Entry = entry;
        Language = language;
        Meta = meta;
    }

    public string Name { get; }
    public IIndexStore Store => _store;
    public PagefindEntry Entry { get; }
    public string Language { get; }
    public MetaIndex Meta { get; }
}
