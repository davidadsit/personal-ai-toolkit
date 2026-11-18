using PersonalDashboard.Kanban;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Page;

public sealed record PageContent(WorkspaceSnapshot Snapshot, Board Board, DateTimeOffset GeneratedAt)
{
    public DateOnly Today => DateOnly.FromDateTime(GeneratedAt.DateTime);
}
