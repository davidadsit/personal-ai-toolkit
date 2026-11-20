using System.Text.Json;
using PersonalDashboard.Feeds;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Weather;

public sealed class OpenMeteoClient(HttpClient http) : IWeatherSource
{
    private const string Forecast = "https://api.open-meteo.com/v1/forecast";

    public async Task<PlaceWeather> Fetch(WeatherPlace place, string timeZone)
    {
        using var response = await http.GetAsync(RequestUri(place, timeZone));
        response.EnsureSuccessStatusCode();
        using var forecast = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var current = forecast.RootElement.GetProperty("current");
        var daily = forecast.RootElement.GetProperty("daily");
        return new PlaceWeather(
            place.Name,
            current.GetProperty("temperature_2m").GetDouble(),
            daily.GetProperty("temperature_2m_max")[0].GetDouble(),
            daily.GetProperty("temperature_2m_min")[0].GetDouble(),
            current.GetProperty("weather_code").GetInt32());
    }

    private static string RequestUri(WeatherPlace place, string timeZone) =>
        FormattableString.Invariant($"{Forecast}?latitude={place.Latitude}&longitude={place.Longitude}&current=temperature_2m,weather_code&daily=temperature_2m_max,temperature_2m_min&temperature_unit=fahrenheit&timezone={Uri.EscapeDataString(timeZone)}&forecast_days=1");
}
