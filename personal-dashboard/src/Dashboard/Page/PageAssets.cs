using PersonalDashboard.Weather;
namespace PersonalDashboard.Page;

public static class PageAssets
{
    private const string ThemePrefix = "themes/";

    public static string Styles { get; } = Read("page.css");

    public static string Script { get; } = Read("page.js");

    public static string FaviconDataUri { get; } = SvgDataUri("favicon.svg");

    public static string? WeatherPictureDataUri(WeatherPicture picture) => picture switch
    {
        WeatherPicture.Sun => SvgDataUri("weather/sun.svg"),
        WeatherPicture.PartSun => SvgDataUri("weather/part-sun.svg"),
        WeatherPicture.Cloud => SvgDataUri("weather/cloud.svg"),
        WeatherPicture.Fog => SvgDataUri("weather/fog.svg"),
        WeatherPicture.Rain => SvgDataUri("weather/rain.svg"),
        WeatherPicture.Snow => SvgDataUri("weather/snow.svg"),
        WeatherPicture.Storm => SvgDataUri("weather/storm.svg"),
        _ => null,
    };

    private static string SvgDataUri(string name) =>
        "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Read(name)));

    public static string Theme(string name)
    {
        var resource = ThemePrefix + name + ".css";
        if (!AvailableThemes().Contains(name))
        {
            throw new InvalidOperationException($"Unknown theme '{name}'. Available themes: {string.Join(", ", AvailableThemes())}, or a themes/{name}.css file in the workspace.");
        }
        return Read(resource);
    }

    private static IReadOnlyList<string> AvailableThemes() =>
        typeof(PageAssets).Assembly.GetManifestResourceNames()
            .Where(resource => resource.StartsWith(ThemePrefix, StringComparison.Ordinal))
            .Select(resource => Path.GetFileNameWithoutExtension(resource))
            .Order(StringComparer.Ordinal)
            .ToList();

    private static string Read(string name)
    {
        using var stream = typeof(PageAssets).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource {name} is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
