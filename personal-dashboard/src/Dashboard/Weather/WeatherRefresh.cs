using System.Globalization;
using System.Text.Json;
using PersonalDashboard.Feeds;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Weather;

public sealed class WeatherRefresh(IWeatherSource source, TimeProvider clock)
{
    public async Task Refresh(WorkspacePaths paths)
    {
        var settings = FeedJson.ReadIfExists<DashboardSettings>(paths.Settings) ?? new DashboardSettings();
        var places = await Task.WhenAll(settings.Weather.ShownPlaces.Select(place => source.Fetch(place, settings.TimeZone)));
        var now = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), settings.TimeZoneInfo);
        var feed = new WeatherFeed(now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), places);

        Directory.CreateDirectory(paths.Output);
        var temporary = paths.Weather + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(feed, FeedJson.Options));
        File.Move(temporary, paths.Weather, overwrite: true);
    }
}
