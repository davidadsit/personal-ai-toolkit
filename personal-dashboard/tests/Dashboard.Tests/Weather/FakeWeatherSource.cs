using PersonalDashboard.Feeds;
using PersonalDashboard.Weather;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Weather;

public sealed class FakeWeatherSource(bool fails = false) : IWeatherSource
{
    public Task<PlaceWeather> Fetch(WeatherPlace place, string timeZone) =>
        fails
            ? throw new HttpRequestException("Weather service unavailable")
            : Task.FromResult(new PlaceWeather(place.Name, 50, 55, 40, 3));
}
