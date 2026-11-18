using PersonalDashboard.Feeds;
using PersonalDashboard.Projects;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class OpenItemsSection
{
    public static string Render(OpenItemsFeed? feed, IReadOnlyList<Project> projects) => $"""
        <section id="open-items">
        <h2 class="label">Open items</h2>
        {Body(feed, projects)}
        </section>
        """;

    private static string Body(OpenItemsFeed? feed, IReadOnlyList<Project> projects)
    {
        if (feed is null || feed.Items.Count == 0)
        {
            return """<p class="muted">Nothing open.</p>""";
        }
        var items = feed.Items.OrderBy(item => item.Raised).Select(item => RenderItem(item, FindProject(item.Project, projects)));
        return $"""<ul>{string.Concat(items)}</ul><p id="no-open-items-for-project" class="muted" hidden>Nothing open for this project.</p>""";
    }

    private static string RenderItem(OpenItem item, Project? project) => $"""
        <li class="open-item" data-projects="{project?.Position}"><strong>{Encode(item.Code)}</strong> <span class="kind">{Encode(item.Kind)}</span> {ToHtml(item.Text)}<div class="muted">Raised {item.Raised:yyyy-MM-dd}{ProjectBlock(item.Project, project)}{FolderBlock(item.Folder)}</div></li>
        """;

    private static Project? FindProject(string? projectId, IReadOnlyList<Project> projects) =>
        projectId is null ? null : projects.FirstOrDefault(candidate => candidate.Title.StartsWith(projectId + ". ", StringComparison.Ordinal));

    private static string FolderBlock(string? folder) =>
        folder is null ? "" : $""" <span class="item-folder">{Encode(folder)}</span>""";

    private static string ProjectBlock(string? projectId, Project? project)
    {
        if (projectId is null)
        {
            return "";
        }
        return project is null
            ? $""" <span class="item-project">{Encode(projectId)}</span>"""
            : $""" <span class="item-project tinted" style="{ProjectColor.Style(project.Position)}">{Encode(project.Title)}</span>""";
    }
}
