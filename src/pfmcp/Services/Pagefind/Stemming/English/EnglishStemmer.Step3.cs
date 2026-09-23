namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static string Step3(string word, int r1, int r2)
    {
        (string From, string To, bool NeedR2)[] replacements =
        [
            ("ational", "ate", false),
            ("tional", "tion", false),
            ("alize", "al", false),
            ("icate", "ic", false),
            ("iciti", "ic", false),
            ("ative", "", true),
            ("ical", "ic", false),
            ("ness", "", false),
            ("ful", "", false),
        ];

        foreach (var (from, to, needR2) in replacements)
        {
            if (!word.EndsWith(from, StringComparison.Ordinal) || word.Length - from.Length < r1)
            {
                continue;
            }

            if (needR2 && word.Length - from.Length < r2)
            {
                continue;
            }

            return word[..^from.Length] + to;
        }

        return word;
    }
}
