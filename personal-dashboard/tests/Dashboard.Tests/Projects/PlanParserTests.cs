using PersonalDashboard.Projects;

namespace PersonalDashboard.Tests.Projects;

public class PlanParserTests
{
    [Fact]
    public void Reads_only_checkbox_lines_inside_the_plan_section()
    {
        var steps = PlanParser.Parse("""
            # Kitchen
            - [ ] Not in the plan
            ## Plan
            Some prose.
            - [ ] First
            - [x] Second
            ## Notes
            - [ ] After the plan
            """);

        Assert.Equal(["First", "Second"], steps.Select(s => s.Text));
    }

    [Fact]
    public void Project_without_a_plan_section_has_no_steps()
    {
        Assert.Empty(PlanParser.Parse("# Kitchen\n- [ ] Loose step\n"));
    }

    [Fact]
    public void Parses_id_now_marker_and_done_date_from_one_line()
    {
        var step = PlanParser.Parse("## Plan\n- [x] P1.2. **Now:** Measure the room. (done 2026-11-12)  \n").Single();

        Assert.Equal("P1.2", step.Id);
        Assert.Equal("Measure the room.", step.Text);
        Assert.True(step.IsFinished);
        Assert.Equal(new DateOnly(2026, 11, 12), step.DoneDate);
    }

    [Fact]
    public void Now_marker_marks_an_open_step_as_underway()
    {
        var step = PlanParser.Parse("## Plan\n- [ ] P1.4. **Now:** Get three quotes\n").Single();

        Assert.True(step.IsNow);
        Assert.Equal("Get three quotes", step.Text);
    }

    [Fact]
    public void Step_without_id_or_done_date_has_neither()
    {
        var step = PlanParser.Parse("## Plan\n- [x] Order cabinets\n").Single();

        Assert.Equal("", step.Id);
        Assert.Null(step.DoneDate);
        Assert.False(step.IsNow);
    }

    [Fact]
    public void Steps_are_numbered_in_file_order()
    {
        var steps = PlanParser.Parse("## Plan\n- [ ] A\n- [ ] B\n- [ ] C\n");

        Assert.Equal([0, 1, 2], steps.Select(s => s.Order));
    }

    [Fact]
    public void Handles_windows_line_endings()
    {
        var steps = PlanParser.Parse("## Plan\r\n- [ ] A\r\n## Notes\r\n- [ ] B\r\n");

        Assert.Equal(["A"], steps.Select(s => s.Text));
    }

    [Fact]
    public void Capital_x_marks_a_finished_step()
    {
        var step = PlanParser.Parse("## Plan\n- [X] Order cabinets\n").Single();

        Assert.True(step.IsFinished);
    }

    [Fact]
    public void Now_marker_on_a_finished_step_is_removed_and_ignored()
    {
        var step = PlanParser.Parse("## Plan\n- [x] **Now:** Order cabinets\n").Single();

        Assert.Equal("Order cabinets", step.Text);
        Assert.False(step.IsNow);
    }
}
