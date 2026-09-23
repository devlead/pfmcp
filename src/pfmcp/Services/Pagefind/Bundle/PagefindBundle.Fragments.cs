namespace Pfmcp.Services.Pagefind;

internal sealed partial class PagefindBundle
{
    public async Task<PageFragment?> GetFragmentAsync(string hash, CancellationToken cancellationToken)
    {
        if (_fragments.TryGetValue(hash, out var cached))
        {
            return cached;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_fragments.TryGetValue(hash, out cached))
            {
                return cached;
            }

            try
            {
                var bytes = await _store.ReadAsync($"fragment/{hash}.pf_fragment", cancellationToken).ConfigureAwait(false);
                cached = PagefindCodec.DecodeFragment(bytes);
                _fragments[hash] = cached;
                return cached;
            }
            catch (Exception)
            {
                return null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}
