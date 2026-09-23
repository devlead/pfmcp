namespace Pfmcp.Services.Pagefind;

internal sealed class FileIndexStore(string source) : IIndexStore
{
    public string Source { get; } = source;

    public Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        var path = Path.Combine(Source, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllBytesAsync(path, cancellationToken);
    }
}
