namespace Pfmcp.Services;

public sealed partial class IndexCatalog(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "pagefind";

    private readonly SemaphoreSlim _httpGate = new(8, 8);
    private readonly List<PagefindBundle> _bundles = [];

    internal IReadOnlyList<PagefindBundle> Bundles => _bundles;
}
