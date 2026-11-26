using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Teatime.Services.Extensions;

/// <summary>Renders the verified extensions as head scripts carrying the request's CSP nonce.</summary>
public static partial class ExtensionHeadRenderer
{
    public static string Build(ExtensionSet extensions, string? nonce)
    {
        if (extensions.IsEmpty)
            return string.Empty;

        var nonceAttr = nonce is { Length: > 0 }
            ? $" nonce=\"{HtmlEncoder.Default.Encode(nonce)}\""
            : string.Empty;

        var sb = new StringBuilder(512);
        foreach (var script in extensions.Active.SelectMany(e => e.Scripts))
        {
            sb.Append("<script").Append(nonceAttr);

            if (script.Async) sb.Append(" async");
            if (script.Defer) sb.Append(" defer");

            if (script.Src is { Length: > 0 } src)
                sb.Append(" src=\"").Append(HtmlEncoder.Default.Encode(src)).Append('"');

            if (script.Attributes is { Count: > 0 } attributes)
                foreach (var (key, value) in attributes)
                    sb.Append(' ').Append(key).Append("=\"").Append(HtmlEncoder.Default.Encode(value)).Append('"');

            // Inline bodies are built from verified settings, so they go out as-is.
            sb.Append('>').Append(script.Inline).Append("</script>").AppendLine();
        }

        foreach (var extension in extensions.Active)
            if (extension.HeadHtml is { Length: > 0 } head)
                sb.AppendLine(StampNonce(head, nonceAttr));

        return sb.ToString();
    }

    /// <summary>Author-written footer HTML from the active extensions, nonced like the head.</summary>
    public static string BuildFooter(ExtensionSet extensions, string? nonce)
    {
        var nonceAttr = nonce is { Length: > 0 }
            ? $" nonce=\"{HtmlEncoder.Default.Encode(nonce)}\""
            : string.Empty;

        return string.Concat(extensions.Active
            .Where(e => e.FooterHtml is { Length: > 0 })
            .Select(e => StampNonce(e.FooterHtml!, nonceAttr) + "\n"));
    }

    // The HTML comes from the site's own extensions.json, so it is trusted: the nonce only lets the CSP admit it.
    private static string StampNonce(string html, string nonceAttr) =>
        nonceAttr.Length == 0 ? html : OpeningTagRegex().Replace(html, m => m.Value + nonceAttr);

    [GeneratedRegex(@"<(?:script|style)\b(?![^>]*\bnonce=)", RegexOptions.IgnoreCase)]
    private static partial Regex OpeningTagRegex();
}
