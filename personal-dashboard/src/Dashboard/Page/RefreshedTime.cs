using System.Globalization;

namespace PersonalDashboard.Page;

public static class RefreshedTime
{
    public static string ToDisplay(string refreshed) =>
        DateTime.TryParseExact(refreshed, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
            ? time.ToString("yyyy-MM-dd h:mm tt", CultureInfo.InvariantCulture)
            : refreshed;
}
