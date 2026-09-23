namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static string Step2(string word, int r1)
    {
        (string From, string To)[] replacements =
        [
            ("ational", "ate"),
            ("fulness", "ful"),
            ("iveness", "ive"),
            ("ization", "ize"),
            ("ousness", "ous"),
            ("biliti", "ble"),
            ("lessli", "less"),
            ("tional", "tion"),
            ("aliti", "al"),
            ("ation", "ate"),
            ("alism", "al"),
            ("entli", "ent"),
            ("fulli", "ful"),
            ("iviti", "ive"),
            ("ousli", "ous"),
            ("enci", "ence"),
            ("anci", "ance"),
            ("abli", "able"),
            ("izer", "ize"),
            ("ator", "ate"),
            ("alli", "al"),
            ("bli", "ble"),
            ("ogi", "og"),
            ("li", ""),
        ];

        foreach (var (from, to) in replacements)
        {
            if (!word.EndsWith(from, StringComparison.Ordinal) || word.Length - from.Length < r1)
            {
                continue;
            }

            if (from == "ogi" && (word.Length < from.Length + 1 || word[^(from.Length + 1)] != 'l'))
            {
                continue;
            }

            if (from == "li" && (word.Length < 3 || !IsValidLi(word[^3])))
            {
                continue;
            }

            return word[..^from.Length] + to;
        }

        return word;
    }
}
