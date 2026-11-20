using System.Text.Json.Serialization;

namespace PersonalDashboard.Workspace;

public sealed record DashboardSettings
{
    public string TimeZone { get; init; } = "America/Denver";
    public string TimeZoneLabel { get; init; } = "MT";
    public int Port { get; init; } = 8771;
    public string CalendarUrl { get; init; } = "https://calendar.google.com/calendar/r";
    public int ReloadSeconds { get; init; } = 15;
    public int DoneShown { get; init; } = 2;
    public int RecentBusinessDays { get; init; } = 2;
    public string Theme { get; init; } = "default";
    public IReadOnlyList<ListPanelSettings> Panels { get; init; } = [];
    public WeatherSettings Weather { get; init; } = new();

    [JsonIgnore]
    public string? WorkspaceThemeCss { get; init; }

    public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
}
