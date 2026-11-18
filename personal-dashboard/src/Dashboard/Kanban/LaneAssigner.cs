using PersonalDashboard.Projects;

namespace PersonalDashboard.Kanban;

public sealed class LaneAssigner(int doneShown, int recentBusinessDays)
{
    public IReadOnlyList<PlacedStep> Assign(Project project, IReadOnlyList<Step> steps, DateOnly today)
    {
        var finished = steps.Where(step => project.IsDone || step.IsFinished).ToList();
        var open = steps.Except(finished).ToList();

        var done = RecentlyDone(finished, today);
        var doing = Doing(project, open);
        var toDo = open.Except(doing).Take(1).ToHashSet();

        return steps.Select(step => new PlacedStep(step, LaneOf(step, finished, done, doing, toDo))).ToList();
    }

    private HashSet<Step> RecentlyDone(IEnumerable<Step> finished, DateOnly today) =>
        finished
            .Where(step => step.DoneDate is { } doneDate && BusinessDays.Between(doneDate, today) <= recentBusinessDays)
            .OrderByDescending(step => step.DoneDate)
            .ThenByDescending(step => step.Order)
            .Take(doneShown)
            .ToHashSet();

    private static HashSet<Step> Doing(Project project, IReadOnlyList<Step> open)
    {
        if (!project.IsInProgress)
        {
            return [];
        }
        var now = open.Where(step => step.IsNow).ToHashSet();
        return now.Count > 0 ? now : open.Take(1).ToHashSet();
    }

    private static Lane LaneOf(Step step, List<Step> finished, HashSet<Step> done, HashSet<Step> doing, HashSet<Step> toDo)
    {
        if (done.Contains(step)) return Lane.Done;
        if (finished.Contains(step)) return Lane.Archive;
        if (doing.Contains(step)) return Lane.Doing;
        if (toDo.Contains(step)) return Lane.ToDo;
        return Lane.Backlog;
    }
}
