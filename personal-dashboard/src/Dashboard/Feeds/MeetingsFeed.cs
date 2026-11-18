namespace PersonalDashboard.Feeds;

public sealed record MeetingsFeed(DateOnly Date, string Refreshed, IReadOnlyList<Meeting> Meetings);
