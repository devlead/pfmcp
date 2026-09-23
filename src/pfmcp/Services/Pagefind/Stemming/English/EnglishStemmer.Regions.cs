namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static int GetR1(string word)
    {
        if (word.StartsWith("gener", StringComparison.Ordinal)
            || word.StartsWith("arsen", StringComparison.Ordinal))
        {
            return 5;
        }

        if (word.StartsWith("commun", StringComparison.Ordinal))
        {
            return 6;
        }

        return RegionAfterFirstVowelConsonant(word, 0);
    }

    private static int GetR2(string word, int r1) => RegionAfterFirstVowelConsonant(word, r1);

    private static int RegionAfterFirstVowelConsonant(string word, int start)
    {
        var i = start;
        while (i < word.Length && !IsVowel(word[i]))
        {
            i++;
        }

        while (i < word.Length && IsVowel(word[i]))
        {
            i++;
        }

        return i < word.Length ? i + 1 : word.Length;
    }
}
