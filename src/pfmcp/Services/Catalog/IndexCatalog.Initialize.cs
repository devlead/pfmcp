namespace Pfmcp.Services;

public sealed partial class IndexCatalog
{
    internal async Task InitializeAsync(IndexSettings settings, CancellationToken cancellationToken)
    {
        _bundles.Clear();
        var specs = BundleLocator.ParseSpecs(settings.Indexes, Environment.GetEnvironmentVariable("PFMCP_INDEXES"));
        if (specs.Count == 0)
        {
            throw new InvalidOperationException("Provide --index <path-or-url> or set PFMCP_INDEXES.");
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var spec in specs)
        {
            var name = spec.Name;
            var suffix = 2;
            while (!names.Add(name))
            {
                name = $"{spec.Name}-{suffix++}";
            }

            var store = CreateStore(spec.Source);
            _bundles.Add(await PagefindBundle.LoadAsync(name, store, cancellationToken).ConfigureAwait(false));
        }
    }

    private IIndexStore CreateStore(string source)
    {
        if (BundleLocator.IsHttp(source))
        {
            return new HttpIndexStore(source, httpClientFactory.CreateClient(HttpClientName), _httpGate);
        }

        return new FileIndexStore(source);
    }
}
