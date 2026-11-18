using PersonalDashboard.Feeds;

namespace PersonalDashboard.Tests.Feeds;

public class MeetingTimeTests
{
    private static readonly TimeZoneInfo Denver = TimeZoneInfo.FindSystemTimeZoneById("America/Denver");

    [Fact]
    public void Converts_local_time_in_standard_time_to_an_instant()
    {
        var instant = MeetingTime.ToInstant(new DateOnly(2026, 11, 17), "09:00", Denver);

        Assert.Equal(new DateTimeOffset(2026, 11, 17, 16, 0, 0, TimeSpan.Zero), instant);
    }

    [Fact]
    public void Converts_local_time_in_daylight_saving_time_to_an_instant()
    {
        var instant = MeetingTime.ToInstant(new DateOnly(2026, 10, 30), "09:00", Denver);

        Assert.Equal(new DateTimeOffset(2026, 10, 30, 15, 0, 0, TimeSpan.Zero), instant);
    }

    [Fact]
    public void Uses_the_offset_of_the_meeting_day_on_the_day_daylight_saving_ends()
    {
        var instant = MeetingTime.ToInstant(new DateOnly(2026, 11, 1), "13:00", Denver);

        Assert.Equal(new DateTimeOffset(2026, 11, 1, 20, 0, 0, TimeSpan.Zero), instant);
    }

    [Fact]
    public void Formats_as_twelve_hour_clock()
    {
        Assert.Equal("1:05 PM", MeetingTime.ToDisplay("13:05"));
        Assert.Equal("12:00 AM", MeetingTime.ToDisplay("00:00"));
    }
}
