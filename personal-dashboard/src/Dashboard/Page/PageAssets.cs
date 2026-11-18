namespace PersonalDashboard.Page;

public static class PageAssets
{
    private const string ThemePrefix = "themes/";

    public static string Styles { get; } = Read("page.css");

    public static string Script { get; } = Read("page.js");

    public static string FaviconDataUri { get; } =
        "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Read("favicon.svg")));

    public static string Theme(string name)
    {
        var resource = ThemePrefix + name + ".css";
        if (!AvailableThemes().Contains(name))
        {
            throw new InvalidOperationException($"Unknown theme '{name}'. Available themes: {string.Join(", ", AvailableThemes())}.");
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
