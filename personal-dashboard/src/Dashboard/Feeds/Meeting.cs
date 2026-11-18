namespace PersonalDashboard.Feeds;

public sealed record Meeting(string Start, string End, string Title, bool Critical, string Reason);
