namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static string Step4(string word, int r2)
    {
        string[] suffixes =
        [
            "ement", "ance", "ence", "able", "ible", "ment",
            "ant", "ent", "ism", "ate", "iti", "ous", "ive", "ize",
            "ion", "al", "er", "ic",
        ];

        foreach (var suffix in suffixes)
        {
            if (!word.EndsWith(suffix, StringComparison.Ordinal) || word.Length - suffix.Length < r2)
            {
                continue;
            }

            if (suffix == "ion" && (word.Length < suffix.Length + 1 || word[^(suffix.Length + 1)] is not ('s' or 't')))
            {
                continue;
            }

            return word[..^suffix.Length];
        }

        return word;
    }
}
