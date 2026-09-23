using System.Globalization;
using System.Text;

namespace Pfmcp.Services.Pagefind;

internal static class QueryTokenizer
{
    public static IReadOnlyList<string> Tokenize(string query, IReadOnlyCollection<string> includeCharacters)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var include = new HashSet<string>(includeCharacters.Where(s => !string.IsNullOrEmpty(s)), StringComparer.Ordinal);
        var builder = new StringBuilder();

        foreach (var rune in query.Normalize(NormalizationForm.FormKC).EnumerateRunes())
        {
            var grapheme = rune.ToString();
            if (include.Contains(grapheme))
            {
                builder.Append(grapheme);
                continue;
            }

            if (IsPagefindPunctuation(rune))
            {
                continue;
            }

            builder.Append(grapheme.ToLowerInvariant());
        }

        var normalized = string.Join(
            ' ',
            builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return string.IsNullOrEmpty(normalized) ? [] : normalized.Split(' ');
    }

    public static string NormalizeDiacritics(string value)
        => string.Concat(value.Normalize(NormalizationForm.FormD).Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

    private static bool IsPagefindPunctuation(Rune rune)
    {
        var category = Rune.GetUnicodeCategory(rune);
        return category is UnicodeCategory.DashPunctuation
            or UnicodeCategory.OpenPunctuation
            or UnicodeCategory.ClosePunctuation
            or UnicodeCategory.InitialQuotePunctuation
            or UnicodeCategory.FinalQuotePunctuation
            or UnicodeCategory.OtherPunctuation;
    }
}
