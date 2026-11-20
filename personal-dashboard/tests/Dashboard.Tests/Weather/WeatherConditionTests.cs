using PersonalDashboard.Weather;

namespace PersonalDashboard.Tests.Weather;

public class WeatherConditionTests
{
    [Theory]
    [InlineData(0, "Clear")]
    [InlineData(2, "Partly cloudy")]
    [InlineData(3, "Cloudy")]
    [InlineData(45, "Fog")]
    [InlineData(53, "Drizzle")]
    [InlineData(63, "Rain")]
    [InlineData(75, "Snow")]
    [InlineData(81, "Showers")]
    [InlineData(86, "Snow showers")]
    [InlineData(95, "Thunderstorm")]
    [InlineData(42, "")]
    public void Names_the_weather_code(int code, string name)
    {
        Assert.Equal(name, WeatherCondition.Name(code));
    }
}
