namespace Pfmcp.Services.Pagefind.Stemming;

/// <summary>
/// Snowball English (Porter 2) as compiled into pagefind_stem / wasm.en.pagefind.
/// </summary>
internal sealed partial class EnglishStemmer : IStemmer
{
    public static EnglishStemmer Instance { get; } = new();

    public string Stem(string lowercaseWord)
    {
        if (string.IsNullOrEmpty(lowercaseWord) || lowercaseWord.Length < 3)
        {
            return lowercaseWord;
        }

        if (Exception1.Contains(lowercaseWord))
        {
            return Exception1Map.TryGetValue(lowercaseWord, out var mapped) ? mapped : lowercaseWord;
        }

        var w = lowercaseWord.ToCharArray();
        if (w[0] == '\'')
        {
            w = w[1..];
        }

        if (w.Length >= 1 && w[0] == 'y')
        {
            w[0] = 'Y';
        }

        for (var i = 1; i < w.Length; i++)
        {
            if (w[i] == 'y' && IsVowel(w[i - 1]))
            {
                w[i] = 'Y';
            }
        }

        var word = new string(w);
        word = StripPossessive(word);

        var r1 = GetR1(word);
        var r2 = GetR2(word, r1);

        word = Step0(word);
        word = Step1a(word);

        if (Exception2.Contains(word))
        {
            return RestoreY(word);
        }

        r1 = Math.Min(r1, word.Length);
        r2 = Math.Min(r2, word.Length);

        word = Step1b(word, r1);
        r1 = Math.Min(r1, word.Length);
        r2 = Math.Min(r2, word.Length);

        word = Step1c(word);
        r1 = Math.Min(r1, word.Length);
        r2 = Math.Min(r2, word.Length);

        word = Step2(word, r1);
        r1 = Math.Min(r1, word.Length);
        r2 = Math.Min(r2, word.Length);

        word = Step3(word, r1, r2);
        r1 = Math.Min(r1, word.Length);
        r2 = Math.Min(r2, word.Length);

        word = Step4(word, r2);
        r1 = Math.Min(r1, word.Length);
        r2 = Math.Min(r2, word.Length);

        word = Step5(word, r1, r2);
        return RestoreY(word);
    }
}
