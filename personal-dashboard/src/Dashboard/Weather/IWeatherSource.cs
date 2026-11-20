using PersonalDashboard.Feeds;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Weather;

public interface IWeatherSource
{
    Task<PlaceWeather> Fetch(WeatherPlace place, string timeZone);
}
