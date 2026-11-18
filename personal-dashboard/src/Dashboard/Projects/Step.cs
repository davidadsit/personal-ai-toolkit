namespace PersonalDashboard.Projects;

public sealed record Step(string Id, string Text, bool IsFinished, DateOnly? DoneDate, bool IsNow, int Order);
