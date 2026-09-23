namespace Pfmcp.Services;

public sealed partial class IndexCatalog
{
    internal PagefindBundle? Find(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return _bundles.Count == 1 ? _bundles[0] : null;
        }

        return _bundles.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    internal IEnumerable<PagefindBundle> Resolve(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return _bundles;
        }

        var bundle = Find(name);
        return bundle is null ? [] : [bundle];
    }
}
