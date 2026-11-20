using PersonalDashboard.Page;

namespace PersonalDashboard.Tests.Page;

public class TemperatureColorTests
{
    [Theory]
    [InlineData(-40, "rgb(140, 175, 250)")]
    [InlineData(-60, "rgb(140, 175, 250)")]
    [InlineData(20, "rgb(255, 255, 255)")]
    [InlineData(40, "rgb(242, 130, 115)")]
    [InlineData(55, "rgb(242, 130, 115)")]
    [InlineData(30, "rgb(249, 193, 185)")]
    public void Runs_from_blue_when_cold_through_white_at_20C_to_red_when_hot(double celsius, string color)
    {
        Assert.Equal(color, TemperatureColor.ForCelsius(celsius));
    }
}
