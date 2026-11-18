using PersonalDashboard.Kanban;
using PersonalDashboard.Projects;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class ProjectListSection
{
    public static string Render(Board board)
    {
        var open = board.Projects.Where(project => !project.IsDone).Select(project => Button(project, project.Title));
        var done = board.Projects.Where(project => project.IsDone).ToList();
        var completed = done.Count == 0 ? "" : $"""
            <details id="completed"><summary>Completed projects <span class="pill">{done.Count}</span></summary>
            <div class="project-list">{string.Concat(done.Select(project => Button(project, $"✓ {project.Title}")))}</div>
            </details>
            """;
        return $"""
            <section id="projects">
            <h2 class="label">Projects</h2>
            <div class="project-list"><button class="project" data-all>All</button>{string.Concat(open)}</div>
            {completed}
            </section>
            """;
    }

    private static string Button(Project project, string title)
    {
        var meta = string.Join(" · ", new[] { project.Status, project.NextDate }.Where(part => part != "").Select(ToHtml));
        return $"""<button class="project tinted" data-project="{project.Position}" style="{ProjectColor.Style(project.Position)}">{Encode(title)}<span class="project-meta">{meta}</span></button>""";
    }
}
