namespace PersonalDashboard.Feeds;

public sealed record EmailsFeed(string Refreshed, IReadOnlyList<Email> Emails);
