namespace PersonalDashboard.Feeds;

public sealed record Email(string From, string Subject, string Received, string Why, string Link, string Thread);
