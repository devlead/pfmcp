namespace Pfmcp.Services.Pagefind;

internal static partial class SearchEngine
{
    private readonly record struct MatchingPageWord(
        PackedPage Page,
        string Stem,
        float LengthBonus,
        float DiacriticBonus,
        int QueryTermIndex);

    private readonly record struct VerboseLoc(
        string Stem,
        byte Weight,
        uint Location,
        float LengthBonus,
        int QueryTermIndex);
}
