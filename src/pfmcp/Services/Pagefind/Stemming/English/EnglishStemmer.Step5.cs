namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static string Step5(string word, int r1, int r2)
    {
        if (word.EndsWith('e'))
        {
            var stem = word[..^1];
            if (stem.Length >= r2 || (stem.Length >= r1 && !IsShortSyllable(stem, stem.Length - 1)))
            {
                return stem;
            }
        }

        if (word.EndsWith("ll", StringComparison.Ordinal) && word.Length - 1 >= r2)
        {
            return word[..^1];
        }

        return word;
    }
}
