namespace Pfmcp.Services.Pagefind;

internal static partial class SearchEngine
{
    public static async Task<PageDocument?> GetPageAsync(
        PagefindBundle bundle,
        string? url,
        string? pageId,
        CancellationToken cancellationToken)
    {
        string? hash = pageId;
        if (string.IsNullOrWhiteSpace(hash) && !string.IsNullOrWhiteSpace(url))
        {
            foreach (var page in bundle.Meta.Pages)
            {
                var fragment = await bundle.GetFragmentAsync(page.Hash, cancellationToken).ConfigureAwait(false);
                if (fragment is not null && UrlsEqual(fragment.Url, url))
                {
                    hash = page.Hash;
                    break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(hash))
        {
            return null;
        }

        var loaded = await bundle.GetFragmentAsync(hash, cancellationToken).ConfigureAwait(false);
        if (loaded is null)
        {
            return null;
        }

        loaded.Meta.TryGetValue("title", out var title);
        var content = loaded.Content;
        var truncated = false;
        if (content.Length > MaxPageContentChars)
        {
            content = content[..MaxPageContentChars] + $"\n… [{loaded.Content.Length - MaxPageContentChars} more characters]";
            truncated = true;
        }

        return new PageDocument
        {
            Index = bundle.Name,
            PageId = hash,
            Url = loaded.Url,
            Title = title ?? loaded.Url,
            Content = content,
            Meta = loaded.Meta,
            Anchors = loaded.Anchors,
            Truncated = truncated
        };
    }

    private static bool UrlsEqual(string left, string right)
        => string.Equals(left.TrimEnd('/'), right.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
}
