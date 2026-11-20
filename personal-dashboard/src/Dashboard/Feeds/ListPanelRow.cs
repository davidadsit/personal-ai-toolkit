namespace PersonalDashboard.Feeds;

public sealed record ListPanelRow(string Title, string Subtitle = "", string Status = "", string Date = "", string Time = "", string Link = "");
