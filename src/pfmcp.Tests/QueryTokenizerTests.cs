namespace Pfmcp.Tests;

public sealed class QueryTokenizerTests
{
    [Fact]
    public Task Keeps_include_characters()
        => Verify(QueryTokenizer.Tokenize("cake_frosting!", ["_"]));

    [Fact]
    public Task Splits_on_punctuation()
        => Verify(QueryTokenizer.Tokenize("Hello, Cake.", []));
}
