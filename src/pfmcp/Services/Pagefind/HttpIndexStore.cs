namespace Pfmcp.Services.Pagefind;

internal sealed class HttpIndexStore(string source, HttpClient http, SemaphoreSlim gate) : IIndexStore
{
    public string Source { get; } = source.EndsWith('/') ? source : source + "/";

    public async Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var uri = new Uri(new Uri(Source), relativePath);
            return await http.GetByteArrayAsync(uri, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }
}
