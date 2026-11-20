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

    [Theory]
    [InlineData(0, WeatherPicture.Sun)]
    [InlineData(1, WeatherPicture.PartSun)]
    [InlineData(3, WeatherPicture.Cloud)]
    [InlineData(48, WeatherPicture.Fog)]
    [InlineData(55, WeatherPicture.Rain)]
    [InlineData(65, WeatherPicture.Rain)]
    [InlineData(82, WeatherPicture.Rain)]
    [InlineData(73, WeatherPicture.Snow)]
    [InlineData(85, WeatherPicture.Snow)]
    [InlineData(99, WeatherPicture.Storm)]
    [InlineData(42, WeatherPicture.None)]
    public void Pictures_the_weather_code(int code, WeatherPicture picture)
    {
        Assert.Equal(picture, WeatherCondition.Picture(code));
    }
}
