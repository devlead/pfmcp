namespace Pfmcp.Services.Pagefind;

internal static class PagefindRanking
{
    public static readonly RankingWeights DefaultWeights = new();

    public static byte LengthDifferential(int keyLength, int termLength)
        => (byte)Math.Min(255, Math.Abs(keyLength - termLength) + 1);

    public static float WordLengthBonus(byte differential, float termSimilarity)
    {
        const float stdDev = 2f;
        var basis = MathF.Exp(-0.5f * (differential * differential) / (stdDev * stdDev));
        var maxValue = MathF.Exp(termSimilarity);
        return MathF.Exp(basis * termSimilarity) / maxValue;
    }

    public static bool DiacriticsMatch(string originalQueryTerm, string variantForm)
        => variantForm == originalQueryTerm
           || variantForm.StartsWith(originalQueryTerm, StringComparison.Ordinal)
           || originalQueryTerm.StartsWith(variantForm, StringComparison.Ordinal);

    public static float DiacriticBonus(string originalQueryTerm, string variantForm, float diacriticSimilarity)
        => diacriticSimilarity > 0 && DiacriticsMatch(originalQueryTerm, variantForm)
            ? 1f + diacriticSimilarity
            : 1f;

    public static float CalculateIdf(int totalPages, int pagesContainingTerm)
        => MathF.Log(((totalPages - pagesContainingTerm + 0.5f) / (pagesContainingTerm + 0.5f)) + 1f);

    public static float Bm25Score(
        float weightedTermFrequency,
        float documentLength,
        float averagePageLength,
        int totalPages,
        int pagesContainingTerm,
        float lengthBonus,
        RankingWeights ranking)
    {
        var weightedWithLength = weightedTermFrequency * lengthBonus;
        var k1 = ranking.TermSaturation;
        var b = ranking.PageLength;
        var avg = averagePageLength <= 0 ? 1f : averagePageLength;
        var idf = CalculateIdf(totalPages, pagesContainingTerm);
        var bm25Tf = (k1 + 1f) * weightedWithLength
                     / (k1 * (1f - b + (b * (documentLength / avg))) + weightedWithLength);
        var rawCountScalar = avg / 5f;
        var rawTf = Math.Min(weightedWithLength / rawCountScalar, k1 + 1f);
        var pagefindTf = ((1f - ranking.TermFrequency) * rawTf) + (ranking.TermFrequency * bm25Tf);
        return idf * pagefindTf;
    }
}
