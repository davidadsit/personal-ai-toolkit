using PersonalDashboard.Feeds;
using PersonalDashboard.Tests.Acceptance;
using PersonalDashboard.Weather;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Weather;

public sealed class WeatherRefreshTests : IDisposable
{
    private static readonly DateTimeOffset Morning = new(2026, 11, 18, 8, 5, 0, TimeSpan.FromHours(-7));
    private readonly string root = Directory.CreateTempSubdirectory("dashboard-weather-").FullName;
    private WorkspacePaths Paths => new(root);

    public void Dispose() => Directory.Delete(root, recursive: true);

    private void Configure(int places) =>
        File.WriteAllText(Paths.Settings, $$"""
            { "timeZone": "America/Denver", "weather": { "places": [ {{string.Join(", ", Enumerable.Range(1, places).Select(n => $$"""{ "name": "Town {{n}}", "latitude": 40, "longitude": -100 }"""))}} ] } }
            """);

    private WeatherFeed ReadFeed() => FeedJson.ReadIfExists<WeatherFeed>(Paths.Weather)!;

    [Fact]
    public async Task Writes_the_weather_of_each_configured_place_with_the_refresh_time()
    {
        Configure(places: 2);

        await new WeatherRefresh(new FakeWeatherSource(), new FixedClock(Morning)).Refresh(Paths);

        var feed = ReadFeed();
        Assert.Equal("2026-11-18 08:05", feed.Refreshed);
        Assert.Equal(["Town 1", "Town 2"], feed.Places.Select(place => place.Name));
    }

    [Fact]
    public async Task Uses_only_the_first_five_places()
    {
        Configure(places: 7);

        await new WeatherRefresh(new FakeWeatherSource(), new FixedClock(Morning)).Refresh(Paths);

        Assert.Equal(["Town 1", "Town 2", "Town 3", "Town 4", "Town 5"], ReadFeed().Places.Select(place => place.Name));
    }

    [Fact]
    public async Task Keeps_the_previous_feed_when_the_weather_service_fails()
    {
        Configure(places: 1);
        Directory.CreateDirectory(Paths.Output);
        File.WriteAllText(Paths.Weather, """{ "refreshed": "2026-11-17 08:00", "places": [] }""");

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            new WeatherRefresh(new FakeWeatherSource(fails: true), new FixedClock(Morning)).Refresh(Paths));

        Assert.Equal("2026-11-17 08:00", ReadFeed().Refreshed);
    }
}
