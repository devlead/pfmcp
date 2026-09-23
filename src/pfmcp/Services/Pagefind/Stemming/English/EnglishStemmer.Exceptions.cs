namespace Pfmcp.Services.Pagefind.Stemming;

internal sealed partial class EnglishStemmer
{
    private static readonly HashSet<string> Exception1 = new(StringComparer.Ordinal)
    {
        "skis", "skies", "dying", "lying", "tying",
        "idly", "gently", "ugly", "early", "only",
        "singly", "sky", "news", "howe", "atlas",
        "cosmos", "bias", "andes",
    };

    private static readonly Dictionary<string, string> Exception1Map = new(StringComparer.Ordinal)
    {
        ["skis"] = "ski",
        ["skies"] = "sky",
        ["dying"] = "die",
        ["lying"] = "lie",
        ["tying"] = "tie",
        ["idly"] = "idl",
        ["gently"] = "gentl",
        ["ugly"] = "ugli",
        ["early"] = "earli",
        ["only"] = "onli",
        ["singly"] = "singl",
    };

    private static readonly HashSet<string> Exception2 = new(StringComparer.Ordinal)
    {
        "inning", "outing", "canning", "herring", "earring",
        "proceed", "exceed", "succeed",
    };
}
