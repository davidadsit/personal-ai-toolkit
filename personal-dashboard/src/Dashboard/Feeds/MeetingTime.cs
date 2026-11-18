using System.Globalization;

namespace PersonalDashboard.Feeds;

public static class MeetingTime
{
    public static DateTimeOffset ToInstant(DateOnly date, string localTime, TimeZoneInfo timeZone)
    {
        var local = date.ToDateTime(ParseTime(localTime));
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local)).ToUniversalTime();
    }

    public static string ToDisplay(string localTime) =>
        ParseTime(localTime).ToString("h:mm tt", CultureInfo.InvariantCulture);

    private static TimeOnly ParseTime(string localTime) => TimeOnly.ParseExact(localTime, "HH:mm", CultureInfo.InvariantCulture);
}
