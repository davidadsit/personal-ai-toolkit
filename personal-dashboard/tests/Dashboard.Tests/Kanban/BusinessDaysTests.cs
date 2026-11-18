using PersonalDashboard.Kanban;

namespace PersonalDashboard.Tests.Kanban;

public class BusinessDaysTests
{
    [Fact]
    public void Weekend_between_friday_and_monday_does_not_count()
    {
        Assert.Equal(1, BusinessDays.Between(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 16)));
    }

    [Fact]
    public void Same_day_is_zero()
    {
        Assert.Equal(0, BusinessDays.Between(new DateOnly(2026, 11, 18), new DateOnly(2026, 11, 18)));
    }

    [Fact]
    public void Counts_weekdays_after_the_date_through_today()
    {
        Assert.Equal(3, BusinessDays.Between(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 18)));
    }

    [Fact]
    public void Date_after_today_is_zero()
    {
        Assert.Equal(0, BusinessDays.Between(new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 18)));
    }
}
