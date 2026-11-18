namespace PersonalDashboard.Feeds;

public sealed record OpenItem(string Code, string Kind, string Text, DateOnly Raised, string? Project = null, string? Folder = null);
