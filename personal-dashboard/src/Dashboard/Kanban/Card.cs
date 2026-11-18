using PersonalDashboard.Projects;

namespace PersonalDashboard.Kanban;

public sealed record Card(Lane Lane, string Text, IReadOnlyList<string> Ids, IReadOnlyList<Project> Projects);
