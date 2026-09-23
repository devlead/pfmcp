namespace Pfmcp.Services.Pagefind;

internal sealed partial class PagefindBundle
{
    public IEnumerable<string> ChunksForTerm(string term)
    {
        var strict = Meta.IndexChunks
            .Where(chunk => string.CompareOrdinal(term, chunk.From) >= 0 && string.CompareOrdinal(term, chunk.To) <= 0)
            .Select(chunk => chunk.Hash)
            .ToList();
        if (strict.Count > 0)
        {
            return strict;
        }

        return Meta.IndexChunks.Where(chunk => LooseMatch(term, chunk)).Select(chunk => chunk.Hash);
    }

    public async Task<WordIndex?> GetChunkAsync(string hash, CancellationToken cancellationToken)
    {
        if (_chunks.TryGetValue(hash, out var cached))
        {
            return cached;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_chunks.TryGetValue(hash, out cached))
            {
                return cached;
            }

            try
            {
                var bytes = await _store.ReadAsync($"index/{hash}.pf_index", cancellationToken).ConfigureAwait(false);
                cached = PagefindCodec.DecodeWordIndex(bytes);
                _chunks[hash] = cached;
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

    private static bool LooseMatch(string term, MetaChunk chunk)
    {
        var fromCount = Math.Min(term.EnumerateRunes().Count(), chunk.From.EnumerateRunes().Count());
        var toCount = Math.Min(term.EnumerateRunes().Count(), chunk.To.EnumerateRunes().Count());
        var termPre = TakeRunes(term, fromCount);
        var chunkPre = TakeRunes(chunk.From, fromCount);
        var termPost = TakeRunes(term, toCount);
        var chunkPost = TakeRunes(chunk.To, toCount);
        return string.CompareOrdinal(termPre, chunkPre) >= 0 && string.CompareOrdinal(termPost, chunkPost) <= 0;
    }

    private static string TakeRunes(string value, int count)
        => string.Concat(value.EnumerateRunes().Take(count));
}
