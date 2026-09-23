namespace Pfmcp.Services.Pagefind;

internal sealed class WordDictionary
{
    private readonly SortedDictionary<string, PackedWord> _words = new(StringComparer.Ordinal);

    public void Add(PackedWord word) => _words[word.Word] = word;

    public void AddRange(IEnumerable<PackedWord> words)
    {
        foreach (var word in words)
        {
            Add(word);
        }
    }

    public IReadOnlyList<(string Key, PackedWord Data)> FindWordExtensions(string term)
    {
        var extensions = new List<(string Key, PackedWord Data)>();
        string? longestPrefix = null;

        foreach (var (key, data) in _words)
        {
            if (key.StartsWith(term, StringComparison.Ordinal))
            {
                extensions.Add((key, data));
            }
            else if (term.StartsWith(key, StringComparison.Ordinal)
                     && key.Length > (longestPrefix?.Length ?? 0))
            {
                longestPrefix = key;
            }
        }

        if (extensions.Count == 0 && longestPrefix is not null && _words.TryGetValue(longestPrefix, out var prefixData))
        {
            extensions.Add((longestPrefix, prefixData));
        }

        return extensions;
    }
}
