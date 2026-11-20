using System.Text.RegularExpressions;
using PersonalDashboard.Feeds;

namespace PersonalDashboard.Workspace;

public sealed partial class VendorIcons(string folder)
{
    private static readonly (string Extension, string MediaType)[] Formats =
        [(".svg", "image/svg+xml"), (".png", "image/png"), (".ico", "image/x-icon")];

    public IReadOnlyDictionary<string, string> For(ListPanelFeed? feed)
    {
        var icons = new Dictionary<string, string>();
        var vendors = (feed?.Rows ?? []).Select(row => row.Icon ?? "").Where(vendor => DomainPattern().IsMatch(vendor)).Distinct();
        foreach (var vendor in vendors)
        {
            if (DataUri(vendor) is { } dataUri)
            {
                icons[vendor] = dataUri;
            }
        }
        return icons;
    }

    private string? DataUri(string vendor)
    {
        foreach (var (extension, mediaType) in Formats)
        {
            var file = Path.Combine(folder, vendor + extension);
            if (File.Exists(file))
            {
                return $"data:{mediaType};base64,{Convert.ToBase64String(File.ReadAllBytes(file))}";
            }
        }
        return null;
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]*(\\.[a-z0-9-]+)+$")]
    private static partial Regex DomainPattern();
}
