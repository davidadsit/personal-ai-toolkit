using PersonalDashboard.Kanban;
using PersonalDashboard.Projects;

namespace PersonalDashboard.Tests.Kanban;

public class LaneAssignerTests
{
    private static readonly DateOnly Wednesday = new(2026, 11, 18);
    private readonly LaneAssigner assigner = new(doneShown: 2, recentBusinessDays: 2);

    private static Project WithStatus(string status) => new("P1. Kitchen", "kitchen.md", status, "", 0);

    private static Step Open(int order, bool isNow = false) => new($"P1.{order}", $"Step {order}", false, null, isNow, order);

    private static Step Finished(int order, DateOnly? doneDate) => new($"P1.{order}", $"Step {order}", true, doneDate, false, order);

    private IReadOnlyList<Lane> LanesOf(Project project, params Step[] steps) =>
        assigner.Assign(project, steps, Wednesday).Select(placed => placed.Lane).ToList();

    [Fact]
    public void In_progress_project_without_now_steps_works_on_its_first_open_step()
    {
        var lanes = LanesOf(WithStatus("In progress"), Open(0), Open(1), Open(2), Open(3));

        Assert.Equal([Lane.Doing, Lane.ToDo, Lane.Backlog, Lane.Backlog], lanes);
    }

    [Fact]
    public void In_progress_project_works_on_every_now_step()
    {
        var lanes = LanesOf(WithStatus("In progress"), Open(0), Open(1, isNow: true), Open(2), Open(3, isNow: true));

        Assert.Equal([Lane.ToDo, Lane.Doing, Lane.Backlog, Lane.Doing], lanes);
    }

    [Fact]
    public void Planned_project_has_nothing_doing_and_its_first_open_step_to_do()
    {
        var lanes = LanesOf(WithStatus("Planned"), Open(0, isNow: true), Open(1));

        Assert.Equal([Lane.ToDo, Lane.Backlog], lanes);
    }

    [Fact]
    public void Steps_done_within_two_business_days_are_done_and_older_ones_archived()
    {
        var lanes = LanesOf(WithStatus("In progress"),
            Finished(0, new DateOnly(2026, 11, 13)),
            Finished(1, new DateOnly(2026, 11, 16)),
            Finished(2, new DateOnly(2026, 11, 17)));

        Assert.Equal([Lane.Archive, Lane.Done, Lane.Done], lanes);
    }

    [Fact]
    public void Only_the_latest_steps_in_the_file_are_done_when_more_finished_today()
    {
        var lanes = LanesOf(WithStatus("In progress"), Finished(0, Wednesday), Finished(1, Wednesday), Finished(2, Wednesday));

        Assert.Equal([Lane.Archive, Lane.Done, Lane.Done], lanes);
    }

    [Fact]
    public void Newer_done_dates_win_over_file_order()
    {
        var lanes = LanesOf(WithStatus("In progress"),
            Finished(0, Wednesday),
            Finished(1, new DateOnly(2026, 11, 17)),
            Finished(2, new DateOnly(2026, 11, 16)));

        Assert.Equal([Lane.Done, Lane.Done, Lane.Archive], lanes);
    }

    [Fact]
    public void Finished_step_without_done_date_is_archived()
    {
        Assert.Equal([Lane.Archive], LanesOf(WithStatus("In progress"), Finished(0, null)));
    }

    [Fact]
    public void Every_step_of_a_done_project_is_finished()
    {
        var lanes = LanesOf(WithStatus("Done"), Open(0), Finished(1, Wednesday));

        Assert.Equal([Lane.Archive, Lane.Done], lanes);
    }

    [Fact]
    public void Placed_steps_keep_file_order()
    {
        var placed = assigner.Assign(WithStatus("In progress"), [Open(0), Finished(1, Wednesday), Open(2)], Wednesday);

        Assert.Equal([0, 1, 2], placed.Select(p => p.Step.Order));
    }
}
