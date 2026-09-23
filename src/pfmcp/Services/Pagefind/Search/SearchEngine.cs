namespace Pfmcp.Services.Pagefind;

internal static partial class SearchEngine
{
    private const int MaxPageContentChars = 8000;
    private static readonly RankingWeights Ranking = PagefindRanking.DefaultWeights;
}
