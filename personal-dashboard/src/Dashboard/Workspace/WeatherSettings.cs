namespace PersonalDashboard.Workspace;

public sealed record WeatherSettings
{
    public const int MaximumPlaces = 5;

    public IReadOnlyList<WeatherPlace> Places { get; init; } = [];
    public bool ConversionStrip { get; init; }

    public IReadOnlyList<WeatherPlace> ShownPlaces => Places.Take(MaximumPlaces).ToList();
}
