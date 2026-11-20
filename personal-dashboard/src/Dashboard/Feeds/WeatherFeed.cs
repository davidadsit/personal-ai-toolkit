namespace PersonalDashboard.Feeds;

public sealed record WeatherFeed(string Refreshed, IReadOnlyList<PlaceWeather> Places);
