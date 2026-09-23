namespace Pfmcp.Services;

public sealed partial class IndexCatalog
{
    internal IReadOnlyList<IndexSummary> Summaries()
        => _bundles.Select(bundle => new IndexSummary
        {
            Name = bundle.Name,
            Source = bundle.Store.Source,
            Version = bundle.Entry.Version,
            Languages = bundle.Entry.Languages.Select(pair => new LanguageSummary
            {
                Language = pair.Key,
                PageCount = pair.Value.PageCount > 0 ? pair.Value.PageCount : bundle.Meta.Pages.Count,
                ChunkCount = bundle.Meta.IndexChunks.Count,
                FilterCount = bundle.Meta.Filters.Count
            }).ToList()
        }).ToList();
}
