namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static string StripPossessive(string word)
    {
        if (word.EndsWith("'s'", StringComparison.Ordinal) || word.EndsWith("'s", StringComparison.Ordinal))
        {
            return word.TrimEnd('\'', 's');
        }

        return word.EndsWith('\'') ? word[..^1] : word;
    }

    private static string Step0(string word)
    {
        if (word.EndsWith("'s'", StringComparison.Ordinal))
        {
            return word[..^3];
        }

        if (word.EndsWith("'s", StringComparison.Ordinal))
        {
            return word[..^2];
        }

        return word.EndsWith('\'') ? word[..^1] : word;
    }
}
