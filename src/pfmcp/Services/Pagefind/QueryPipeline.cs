namespace Pfmcp.Services.Pagefind;

internal sealed record QueryTerm(string Original, string Stem);

internal static class QueryPipeline
{
    public static IReadOnlyList<QueryTerm> Analyze(
        string query,
        IReadOnlyCollection<string> includeCharacters,
        IStemmer stemmer)
    {
        var tokens = QueryTokenizer.Tokenize(query, includeCharacters);
        if (tokens.Count == 0)
        {
            return [];
        }

        return tokens
            .Select(token => new QueryTerm(token, stemmer.Stem(QueryTokenizer.NormalizeDiacritics(token))))
            .Where(term => term.Stem.Length > 0)
            .ToList();
    }

    public static IEnumerable<string> IndexKeys(IEnumerable<QueryTerm> terms)
    {
        foreach (var term in terms)
        {
            yield return term.Original;
            if (!string.Equals(term.Original, term.Stem, StringComparison.Ordinal))
            {
                yield return term.Stem;
            }
        }
    }
}
