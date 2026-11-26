using Teatime.Models;

namespace Teatime.Services.Extensions;

public static partial class ExtensionLoader
{
    private static ActiveExtension? BuildUmami(UmamiOptions? options, Rejections rejects)
    {
        if (options is not { Enabled: true })
            return null;

        if (!TryBaseUrl(options.Url, out var baseUrl, out var origin))
            return Reject("umami", "\"url\" must be an absolute http(s) URL", rejects);

        if (!Guid.TryParse(options.WebsiteId, out var websiteId))
            return Reject("umami", "\"websiteId\" must be the website id (a UUID) from Umami", rejects);

        KeyValuePair<string, string>[] attributes = [new("data-website-id", websiteId.ToString())];
        var tracker = new ExtensionScript(Src: $"{baseUrl}/script.js", Defer: true, Attributes: attributes);

        // recorder.js adds session replays and heatmaps on top of the tracker; it never replaces it.
        ExtensionScript[] scripts = options.Recorder
            ? [tracker, new ExtensionScript(Src: $"{baseUrl}/recorder.js", Defer: true, Attributes: attributes)]
            : [tracker];

        return new ActiveExtension("umami", scripts, [origin]);
    }
}
