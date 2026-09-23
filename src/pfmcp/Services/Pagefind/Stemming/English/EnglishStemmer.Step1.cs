namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static string Step1a(string word)
    {
        if (word.EndsWith("sses", StringComparison.Ordinal))
        {
            return word[..^2];
        }

        if (word.EndsWith("ied", StringComparison.Ordinal) || word.EndsWith("ies", StringComparison.Ordinal))
        {
            return word.Length > 4 ? word[..^2] : word[..^1];
        }

        if (word.EndsWith("us", StringComparison.Ordinal) || word.EndsWith("ss", StringComparison.Ordinal))
        {
            return word;
        }

        if (word.EndsWith('s') && HasVowel(word.AsSpan(0, word.Length - 2)))
        {
            return word[..^1];
        }

        return word;
    }

    private static string Step1b(string word, int r1)
    {
        if (word.EndsWith("eedly", StringComparison.Ordinal) || word.EndsWith("eed", StringComparison.Ordinal))
        {
            var suffix = word.EndsWith("eedly", StringComparison.Ordinal) ? 5 : 3;
            return word.Length - suffix >= r1 ? word[..^suffix] + "ee" : word;
        }

        string[] suffixes = ["ingly", "edly", "ing", "ed"];
        foreach (var suffix in suffixes)
        {
            if (!word.EndsWith(suffix, StringComparison.Ordinal))
            {
                continue;
            }

            var stem = word[..^suffix.Length];
            if (!HasVowel(stem))
            {
                return word;
            }

            word = stem;
            if (word.EndsWith("at", StringComparison.Ordinal)
                || word.EndsWith("bl", StringComparison.Ordinal)
                || word.EndsWith("iz", StringComparison.Ordinal))
            {
                return word + "e";
            }

            if (EndsWithDouble(word) && !EndsWithAny(word, "ll", "ss", "zz"))
            {
                return word[..^1];
            }

            if (IsShort(word, GetR1(word)))
            {
                return word + "e";
            }

            return word;
        }

        return word;
    }

    private static string Step1c(string word)
    {
        if (word.Length > 2
            && (word[^1] is 'y' or 'Y')
            && !IsVowel(word[^2]))
        {
            return word[..^1] + "i";
        }

        return word;
    }
}
