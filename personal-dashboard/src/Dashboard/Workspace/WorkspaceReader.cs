using PersonalDashboard.Feeds;
using PersonalDashboard.Kanban;
using PersonalDashboard.Projects;

namespace PersonalDashboard.Workspace;

public sealed class WorkspaceReader(string root)
{
    private readonly WorkspacePaths paths = new(root);

    public WorkspaceSnapshot Read()
    {
        var settings = FeedJson.ReadIfExists<DashboardSettings>(paths.Settings) ?? new DashboardSettings();
        return new(
            settings,
            ReadPlans(),
            FeedJson.ReadIfExists<MeetingsFeed>(paths.Meetings),
            FeedJson.ReadIfExists<EmailsFeed>(paths.Emails),
            FeedJson.ReadIfExists<OpenItemsFeed>(paths.OpenItems),
            ReadListPanels(settings.Panels),
            FeedJson.ReadIfExists<WeatherFeed>(paths.Weather));
    }

    private IReadOnlyList<ListPanel> ReadListPanels(IReadOnlyList<ListPanelSettings> panels)
    {
        FeedNames.EnsureValid(panels);
        var icons = new VendorIcons(paths.Icons);
        return panels.Select(panel =>
        {
            var feed = FeedJson.ReadIfExists<ListPanelFeed>(paths.ListPanelFeed(panel.Feed));
            return new ListPanel(panel, feed, icons.For(feed));
        }).ToList();
    }

    private IReadOnlyList<ProjectPlan> ReadPlans()
    {
        if (!File.Exists(paths.ProjectIndex))
        {
            return [];
        }
        return ProjectIndexParser.Parse(File.ReadAllText(paths.ProjectIndex))
            .Select(project => new ProjectPlan(project, ReadSteps(project)))
            .ToList();
    }

    private IReadOnlyList<Step> ReadSteps(Project project)
    {
        var file = Path.Combine(paths.Projects, project.File);
        return File.Exists(file) ? PlanParser.Parse(File.ReadAllText(file)) : [];
    }
}
