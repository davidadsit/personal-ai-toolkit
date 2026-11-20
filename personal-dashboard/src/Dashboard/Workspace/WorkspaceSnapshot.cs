using PersonalDashboard.Feeds;
using PersonalDashboard.Kanban;

namespace PersonalDashboard.Workspace;

public sealed record WorkspaceSnapshot(
    DashboardSettings Settings,
    IReadOnlyList<ProjectPlan> Plans,
    MeetingsFeed? Meetings,
    EmailsFeed? Emails,
    OpenItemsFeed? OpenItems,
    IReadOnlyList<ListPanel> ListPanels);
