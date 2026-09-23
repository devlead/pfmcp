namespace Pfmcp.Services.Pagefind;

internal sealed partial class PagefindBundle
{
    public IReadOnlyDictionary<string, FilterIndex> Filters => _filters;

    public async Task EnsureFiltersAsync(CancellationToken cancellationToken)
    {
        if (_filters.Count > 0 || Meta.Filters.Count == 0)
        {
            return;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_filters.Count > 0)
            {
                return;
            }

            foreach (var filter in Meta.Filters)
            {
                try
                {
                    var bytes = await _store.ReadAsync($"filter/{filter.Hash}.pf_filter", cancellationToken).ConfigureAwait(false);
                    var decoded = PagefindCodec.DecodeFilterIndex(bytes);
                    _filters[decoded.Filter] = decoded;
                }
                catch (Exception)
                {
                    _filters[filter.Filter] = new FilterIndex { Filter = filter.Filter, Values = [] };
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}
