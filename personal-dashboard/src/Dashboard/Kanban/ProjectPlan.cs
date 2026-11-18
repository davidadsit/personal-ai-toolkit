using PersonalDashboard.Projects;

namespace PersonalDashboard.Kanban;

public sealed record ProjectPlan(Project Project, IReadOnlyList<Step> Steps);
