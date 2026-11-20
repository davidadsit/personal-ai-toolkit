namespace PersonalDashboard.Feeds;

public sealed record ListPanelFeed(string Refreshed, IReadOnlyList<ListPanelRow> Rows);
