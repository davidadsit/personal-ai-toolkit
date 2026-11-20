using System.Globalization;
using PersonalDashboard.Feeds;
using PersonalDashboard.Weather;
using PersonalDashboard.Workspace;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class WeatherBanner
{
    private const int StripStep = 5;
    private const int StripLimit = 40;

    public static string Render(WeatherSettings settings, WeatherFeed? feed)
    {
        var parts = (settings.Places.Count > 0 ? Places(feed) : "") + (settings.ConversionStrip ? ConversionStrip() : "");
        return parts == "" ? "" : $"""<header id="banner"><div class="banner-pill">{parts}</div></header>""";
    }

    private static string ConversionStrip()
    {
        var cells = Enumerable.Range(-StripLimit / StripStep, 2 * StripLimit / StripStep + 1)
            .Select(step => step * StripStep)
            .Select(celsius => $"""<span class="strip-cell" style="background: {TemperatureColor.ForCelsius(celsius)}">{Degrees(celsius)}°C / {Degrees(celsius * 9 / 5 + 32)}°F</span>""");
        return $"""<div class="conversion-strip">{string.Concat(cells)}</div>""";
    }

    private static string Places(WeatherFeed? feed)
    {
        if (feed is null)
        {
            return """<div class="weather"><span class="muted">Weather: no data yet. Run /dashboard.</span></div>""";
        }
        return $"""<div class="weather">{string.Concat((feed.Places ?? []).Select(Place))}</div>""";
    }

    private static string Place(PlaceWeather place)
    {
        var color = TemperatureColor.ForCelsius(TemperatureColor.ToCelsius(place.Temperature));
        var condition = WeatherCondition.Name(place.Code);
        var summary = condition == "" ? "" : $" {condition}";
        return $"""<span class="weather-place" style="background: {color}"><strong>{Encode(place.Name ?? "")}</strong> {Degrees(place.Temperature)}°F{summary} · H {Degrees(place.High)}° L {Degrees(place.Low)}°</span>""";
    }

    private static string Degrees(double value)
    {
        var rounded = (int)Math.Round(value, MidpointRounding.AwayFromZero);
        return rounded < 0 ? "−" + (-rounded).ToString(CultureInfo.InvariantCulture) : rounded.ToString(CultureInfo.InvariantCulture);
    }
}
