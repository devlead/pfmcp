namespace Pfmcp.Services.Pagefind.Stemming;

internal interface IStemmer
{
    string Stem(string lowercaseWord);
}
