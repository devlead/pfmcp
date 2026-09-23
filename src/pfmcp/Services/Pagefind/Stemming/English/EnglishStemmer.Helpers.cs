namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static bool IsShort(string word, int r1) => r1 >= word.Length && IsShortSyllable(word, word.Length - 1);

    private static bool IsShortSyllable(string word, int i)
    {
        if (i < 0 || i >= word.Length)
        {
            return false;
        }

        if (i == 0)
        {
            return IsVowel(word[0]) && word.Length > 1 && !IsVowel(word[1]);
        }

        return !IsVowel(word[i])
            && IsVowel(word[i - 1])
            && (i < 2 || !IsVowel(word[i - 2]))
            && word[i] is not ('w' or 'x' or 'Y');
    }

    private static bool EndsWithDouble(string word)
        => word.Length >= 2 && word[^1] == word[^2] && "bdfgmnprt".Contains(word[^1]);

    private static bool EndsWithAny(string word, params string[] suffixes)
        => suffixes.Any(s => word.EndsWith(s, StringComparison.Ordinal));

    private static bool IsValidLi(char c) => "cdeghkmnrt".Contains(c);

    private static bool HasVowel(ReadOnlySpan<char> span)
    {
        foreach (var c in span)
        {
            if (IsVowel(c))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';

    private static string RestoreY(string word)
    {
        if (word.IndexOf('Y') < 0)
        {
            return word;
        }

        return word.Replace('Y', 'y');
    }
}
