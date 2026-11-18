namespace PersonalDashboard.Kanban;

public static class BusinessDays
{
    public static int Between(DateOnly date, DateOnly today)
    {
        var count = 0;
        for (var day = date.AddDays(1); day <= today; day = day.AddDays(1))
        {
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                count++;
            }
        }
        return count;
    }
}
