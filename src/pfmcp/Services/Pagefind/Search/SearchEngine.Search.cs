namespace Pfmcp.Services.Pagefind;

internal static partial class SearchEngine
{
    public static async Task<IReadOnlyList<SearchHit>> SearchAsync(
        PagefindBundle bundle,
        string query,
        IReadOnlyDictionary<string, string>? filters,
        int limit,
        CancellationToken cancellationToken)
    {
        var stemmer = StemmerFactory.Create(bundle.Language);
        var terms = QueryPipeline.Analyze(query, bundle.Entry.IncludeCharacters, stemmer);
        if (terms.Count == 0)
        {
            return [];
        }

        if (filters is { Count: > 0 })
        {
            await bundle.EnsureFiltersAsync(cancellationToken).ConfigureAwait(false);
        }

        var hashes = QueryPipeline.IndexKeys(terms)
            .SelectMany(bundle.ChunksForTerm)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var dictionary = new WordDictionary();
        foreach (var hash in hashes)
        {
            var chunk = await bundle.GetChunkAsync(hash, cancellationToken).ConfigureAwait(false);
            if (chunk is not null)
            {
                dictionary.AddRange(chunk.Words);
            }
        }

        var matchingWords = new List<MatchingPageWord>();
        var combinedPageCounts = new int[terms.Count];
        var termPageSets = new List<HashSet<uint>>();

        for (var termIndex = 0; termIndex < terms.Count; termIndex++)
        {
            var term = terms[termIndex];
            var union = new HashSet<uint>();
            foreach (var (key, data) in dictionary.FindWordExtensions(term.Stem))
            {
                var lengthBonus = PagefindRanking.WordLengthBonus(
                    PagefindRanking.LengthDifferential(key.Length, term.Stem.Length),
                    Ranking.TermSimilarity);

                var variants = new List<(string Form, List<PackedPage> Pages)>();
                variants.Add((key, data.Pages));
                variants.AddRange(data.AdditionalVariants.Select(v => (v.Form, v.Pages)));

                foreach (var (form, pages) in variants)
                {
                    var boost = PagefindRanking.DiacriticBonus(term.Original, form, Ranking.DiacriticSimilarity);
                    foreach (var page in pages)
                    {
                        matchingWords.Add(new MatchingPageWord(page, key, lengthBonus, boost, termIndex));
                        union.Add(page.PageNumber);
                    }
                }
            }

            combinedPageCounts[termIndex] = union.Count;
            if (union.Count > 0)
            {
                termPageSets.Add(union);
            }
        }

        if (termPageSets.Count == 0)
        {
            return [];
        }

        HashSet<uint> candidates = [.. termPageSets[0]];
        for (var i = 1; i < termPageSets.Count; i++)
        {
            candidates.IntersectWith(termPageSets[i]);
        }

        if (filters is { Count: > 0 })
        {
            var allowed = ApplyFilters(bundle, filters);
            if (allowed.Count == 0)
            {
                return [];
            }

            candidates.IntersectWith(allowed);
        }

        var totalPages = bundle.Meta.Pages.Count;
        var averagePageLength = bundle.Meta.AveragePageLength;
        var queryTotalIdf = combinedPageCounts.Sum(count => PagefindRanking.CalculateIdf(totalPages, count));

        var hits = new List<SearchHit>();
        foreach (var pageNumber in candidates)
        {
            if (pageNumber >= bundle.Meta.Pages.Count)
            {
                continue;
            }

            var page = bundle.Meta.Pages[(int)pageNumber];
            var pageWords = matchingWords.Where(w => w.Page.PageNumber == pageNumber).ToList();
            var locations = new List<VerboseLoc>();
            var metaFieldMatches = new Dictionary<ushort, Dictionary<int, (string Stem, float Idf)>>();

            foreach (var match in pageWords)
            {
                foreach (var (weight, location) in match.Page.Locs)
                {
                    locations.Add(new VerboseLoc(match.Stem, weight, location, match.LengthBonus, match.QueryTermIndex));
                }

                if (match.Page.MetaLocs.Count == 0)
                {
                    continue;
                }

                var idf = PagefindRanking.CalculateIdf(totalPages, combinedPageCounts[match.QueryTermIndex]);
                foreach (var (fieldId, _) in match.Page.MetaLocs)
                {
                    if (!metaFieldMatches.TryGetValue(fieldId, out var byTerm))
                    {
                        byTerm = [];
                        metaFieldMatches[fieldId] = byTerm;
                    }

                    byTerm.TryAdd(match.QueryTermIndex, (match.Stem, idf));
                }
            }

            locations.Sort((a, b) => a.Location.CompareTo(b.Location));

            var weightedWords = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var excerptLocations = new List<uint>();
            var groups = locations
                .GroupBy(l => l.Location)
                .Select(g => g.ToList())
                .ToList();

            foreach (var group in groups)
            {
                var seenTerms = new SortedDictionary<int, (string Stem, byte Weight, float LengthBonus)>();
                foreach (var loc in group)
                {
                    seenTerms.TryAdd(loc.QueryTermIndex, (loc.Stem, loc.Weight, loc.LengthBonus));
                }

                var minWeight = seenTerms.Count == 0 ? (byte)1 : seenTerms.Values.Min(v => v.Weight);
                var effectiveWeight = seenTerms.Count > 1
                    ? (byte)Math.Min(255, minWeight * seenTerms.Count)
                    : minWeight;

                foreach (var (_, (stem, _, _)) in seenTerms)
                {
                    weightedWords[stem] = weightedWords.GetValueOrDefault(stem) + effectiveWeight;
                }

                excerptLocations.Add(group[0].Location);
            }

            float baseScore = 0;
            foreach (var (stem, weightSum) in weightedWords)
            {
                var matched = pageWords.First(w => w.Stem == stem);
                baseScore += PagefindRanking.Bm25Score(
                    weightSum / 24f,
                    page.WordCount,
                    averagePageLength,
                    totalPages,
                    combinedPageCounts[matched.QueryTermIndex],
                    matched.LengthBonus,
                    Ranking) * matched.DiacriticBonus;
            }

            float metaBoost = 0;
            foreach (var (fieldId, wordIdfs) in metaFieldMatches)
            {
                var fieldName = fieldId < bundle.Meta.MetaFields.Count ? bundle.Meta.MetaFields[fieldId] : null;
                var fieldWeight = fieldName is not null && Ranking.MetaWeights.TryGetValue(fieldName, out var configured)
                    ? configured
                    : 1f;

                var matchedIdf = wordIdfs.Values.Sum(v => v.Idf);
                if (queryTotalIdf <= 0)
                {
                    continue;
                }

                var coverage = matchedIdf / queryTotalIdf;
                metaBoost += fieldWeight * matchedIdf * coverage * coverage;
            }

            var fragment = await bundle.GetFragmentAsync(page.Hash, cancellationToken).ConfigureAwait(false);
            if (fragment is null)
            {
                continue;
            }

            fragment.Meta.TryGetValue("title", out var title);
            hits.Add(new SearchHit
            {
                Index = bundle.Name,
                Language = bundle.Language,
                PageId = page.Hash,
                Url = fragment.Url,
                Title = title ?? fragment.Url,
                Excerpt = ExcerptBuilder.Build(fragment.Content, excerptLocations),
                Score = baseScore + metaBoost
            });
        }

        return hits
            .OrderByDescending(h => h.Score)
            .ThenBy(h => h.Title, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, limit))
            .ToList();
    }
}
