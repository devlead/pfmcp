namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed class IdentityStemmer : IStemmer
{
    public static IdentityStemmer Instance { get; } = new();

    public string Stem(string lowercaseWord) => lowercaseWord;
}
