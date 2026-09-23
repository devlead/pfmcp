namespace Pfmcp.Services.Pagefind.Stemming;

internal static class StemmerFactory
{
    public static IStemmer Create(string? languageOrWasm)
    {
        var key = (languageOrWasm ?? "en").Split('-', 2)[0].ToLowerInvariant();
        return key is "en" or "eng" ? EnglishStemmer.Instance : IdentityStemmer.Instance;
    }
}
