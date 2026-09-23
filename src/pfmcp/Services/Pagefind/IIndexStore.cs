namespace Pfmcp.Services.Pagefind;

internal interface IIndexStore
{
    string Source { get; }
    Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken);
}
