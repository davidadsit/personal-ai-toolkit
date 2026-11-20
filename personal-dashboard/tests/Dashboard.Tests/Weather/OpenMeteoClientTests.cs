using PersonalDashboard.Feeds;
using System.Net;
using PersonalDashboard.Weather;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Weather;

public class OpenMeteoClientTests
{
    private const string Forecast = """
        { "current": { "time": "2026-11-20T09:00", "temperature_2m": 41.2, "weather_code": 0 },
          "daily": { "time": ["2026-11-20"], "temperature_2m_max": [48.1], "temperature_2m_min": [29.3], "weather_code": [3] } }
        """;

    [Fact]
    public async Task Asks_for_current_and_daily_weather_in_fahrenheit_for_the_place()
    {
        var handler = new StubHttpHandler(HttpStatusCode.OK, Forecast);

        await new OpenMeteoClient(new HttpClient(handler)).Fetch(new WeatherPlace("Springfield", 39.8, -89.65), "America/Chicago");

        var request = handler.Requests.Single();
        Assert.Equal("api.open-meteo.com", request.Host);
        Assert.Contains("latitude=39.8", request.Query);
        Assert.Contains("longitude=-89.65", request.Query);
        Assert.Contains("temperature_unit=fahrenheit", request.Query);
        Assert.Contains("timezone=America%2FChicago", request.Query);
    }

    [Fact]
    public async Task Reads_the_current_temperature_and_condition_and_todays_high_and_low()
    {
        var client = new OpenMeteoClient(new HttpClient(new StubHttpHandler(HttpStatusCode.OK, Forecast)));

        var weather = await client.Fetch(new WeatherPlace("Springfield", 39.8, -89.65), "America/Chicago");

        Assert.Equal(new PlaceWeather("Springfield", 41.2, 48.1, 29.3, 0), weather);
    }

    [Fact]
    public async Task A_failed_request_throws()
    {
        var client = new OpenMeteoClient(new HttpClient(new StubHttpHandler(HttpStatusCode.ServiceUnavailable, "")));

        await Assert.ThrowsAsync<HttpRequestException>(() => client.Fetch(new WeatherPlace("Springfield", 39.8, -89.65), "America/Chicago"));
    }
}
