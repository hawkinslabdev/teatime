using Teatime.Models;

namespace Teatime.Services.Extensions;

public static partial class ExtensionLoader
{
    private static ActiveExtension? BuildCustom(CustomCodeOptions? options, Rejections rejects)
    {
        if (options is not { Enabled: true })
            return null;

        var head = Coalesce(options.Head);
        var footer = Coalesce(options.Footer);
        if (head is null && footer is null)
            return Reject("custom", "set \"head\", \"footer\" or both", rejects);

        var origins = new List<string>();
        foreach (var source in options.Sources ?? [])
        {
            if (!TryBaseUrl(source, out _, out var origin))
                return Reject("custom", $"\"sources\" entry \"{source}\" must be an absolute http(s) URL", rejects);
            origins.Add(origin);
        }

        return new ActiveExtension("custom", [], origins, head, footer);
    }
}
