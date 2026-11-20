using PersonalDashboard.Feeds;

namespace PersonalDashboard.Workspace;

public sealed record ListPanel(ListPanelSettings Settings, ListPanelFeed? Feed);
