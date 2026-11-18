using PersonalDashboard.Kanban;
using PersonalDashboard.Page;

namespace PersonalDashboard.Workspace;

public sealed class DashboardGenerator(TimeProvider clock)
{
    public void Generate(WorkspacePaths paths)
    {
        var snapshot = new WorkspaceReader(paths.Root).Read();
        var settings = snapshot.Settings;
        var now = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), settings.TimeZoneInfo);
        var board = new BoardBuilder(new LaneAssigner(settings.DoneShown, settings.RecentBusinessDays))
            .Build(snapshot.Plans, DateOnly.FromDateTime(now.DateTime));

        Directory.CreateDirectory(paths.Output);
        var temporary = paths.Page + ".tmp";
        File.WriteAllText(temporary, PageRenderer.Render(new PageContent(snapshot, board, now)));
        File.Move(temporary, paths.Page, overwrite: true);
    }
}
