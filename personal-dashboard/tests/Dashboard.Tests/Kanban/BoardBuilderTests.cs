using PersonalDashboard.Kanban;
using PersonalDashboard.Projects;

namespace PersonalDashboard.Tests.Kanban;

public class BoardBuilderTests
{
    private static readonly DateOnly Wednesday = new(2026, 11, 18);
    private readonly BoardBuilder builder = new(new LaneAssigner(doneShown: 2, recentBusinessDays: 2));

    private static ProjectPlan Plan(int position, string nextDate, params string[] openSteps) =>
        new(new Project($"P{position + 1}. Project", $"p{position}.md", "Planned", nextDate, position),
            openSteps.Select((text, order) => new Step($"P{position + 1}.{order + 1}", text, false, null, false, order)).ToList());

    [Fact]
    public void Dated_projects_come_first_by_date_then_undated_in_table_order()
    {
        var board = builder.Build([
            Plan(0, "Unknown"),
            Plan(1, "2027-03-01"),
            Plan(2, ""),
            Plan(3, "2026-12-01"),
        ], Wednesday);

        Assert.Equal([3, 1, 0, 2], board.Projects.Select(p => p.Position));
    }

    [Fact]
    public void Cards_follow_project_order_then_step_order()
    {
        var board = builder.Build([Plan(0, "", "A1", "A2", "A3"), Plan(1, "2026-12-01", "B1", "B2", "B3")], Wednesday);

        Assert.Equal(["B2", "B3", "A2", "A3"], board.CardsIn(Lane.Backlog).Select(c => c.Text));
    }

    [Fact]
    public void Identical_steps_from_different_projects_in_one_lane_stack_into_one_card()
    {
        var board = builder.Build([Plan(0, "", "Call the bank"), Plan(1, "", "Call the bank")], Wednesday);

        var card = Assert.Single(board.CardsIn(Lane.ToDo));
        Assert.Equal(["P1. Project", "P2. Project"], card.Projects.Select(p => p.Title));
        Assert.Equal(["P1.1", "P2.1"], card.Ids);
    }

    [Fact]
    public void Identical_steps_in_different_lanes_do_not_stack()
    {
        var board = builder.Build([Plan(0, "", "First", "Call the bank"), Plan(1, "", "Call the bank")], Wednesday);

        Assert.Equal(["Call the bank"], board.CardsIn(Lane.Backlog).Select(c => c.Text));
        Assert.Equal(["First", "Call the bank"], board.CardsIn(Lane.ToDo).Select(c => c.Text));
    }

    [Fact]
    public void Identical_steps_within_one_project_do_not_stack()
    {
        var board = builder.Build([Plan(0, "", "First", "Repeat", "Repeat")], Wednesday);

        Assert.Equal(2, board.CardsIn(Lane.Backlog).Count);
    }

    [Fact]
    public void Stacked_card_takes_the_position_of_its_first_project()
    {
        var board = builder.Build([Plan(0, "", "x", "Shared"), Plan(1, "", "y", "Other"), Plan(2, "", "z", "Shared")], Wednesday);

        Assert.Equal(["Shared", "Other"], board.CardsIn(Lane.Backlog).Select(c => c.Text));
    }

    [Fact]
    public void Steps_without_ids_add_no_id_to_the_card()
    {
        var plan = new ProjectPlan(new Project("P1", "p.md", "Planned", "", 0), [new Step("", "Loose", false, null, false, 0)]);

        Assert.Empty(builder.Build([plan], Wednesday).CardsIn(Lane.ToDo).Single().Ids);
    }
}
