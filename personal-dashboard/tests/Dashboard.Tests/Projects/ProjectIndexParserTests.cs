using PersonalDashboard.Projects;

namespace PersonalDashboard.Tests.Projects;

public class ProjectIndexParserTests
{
    private const string Index = """
        # Projects

        Some prose about projects.

        | Project | Horizon | Status | Partners | Next date | Outcome | Notes |
        |---|---|---|---|---|---|---|
        | [P1. Kitchen remodel](kitchen.md) | Short | In progress | Contractor | 2027-03-01 | | |
        | [P2. Learn Rust](rust.md) | Long | Planned | | Unknown | | |
        | [P3. Tax return](taxes.md) | Short | Done | | | Filed | |
        """;

    [Fact]
    public void Reads_only_rows_that_start_with_a_link()
    {
        var projects = ProjectIndexParser.Parse(Index);

        Assert.Equal(["P1. Kitchen remodel", "P2. Learn Rust", "P3. Tax return"], projects.Select(p => p.Title));
    }

    [Fact]
    public void Reads_file_status_and_next_date()
    {
        var kitchen = ProjectIndexParser.Parse(Index)[0];

        Assert.Equal("kitchen.md", kitchen.File);
        Assert.Equal("In progress", kitchen.Status);
        Assert.Equal("2027-03-01", kitchen.NextDate);
    }

    [Fact]
    public void Numbers_projects_by_their_position_in_the_table()
    {
        var projects = ProjectIndexParser.Parse(Index);

        Assert.Equal([0, 1, 2], projects.Select(p => p.Position));
    }

    [Fact]
    public void Due_date_is_the_first_iso_date_in_the_next_date_cell()
    {
        var project = new Project("T", "t.md", "Planned", "Due 2026-12-01 (draft), then 2027-01-01", 0);

        Assert.Equal(new DateOnly(2026, 12, 1), project.DueDate);
    }

    [Fact]
    public void Project_without_a_date_has_no_due_date()
    {
        var project = new Project("T", "t.md", "Planned", "Unknown", 0);

        Assert.Null(project.DueDate);
    }

    [Theory]
    [InlineData("Done", true)]
    [InlineData("Done, filed", true)]
    [InlineData("In progress", false)]
    public void Project_is_done_when_status_starts_with_done(string status, bool expected)
    {
        Assert.Equal(expected, new Project("T", "t.md", status, "", 0).IsDone);
    }

    [Theory]
    [InlineData("In progress", true)]
    [InlineData("In progress, blocked", true)]
    [InlineData("Planned", false)]
    public void Project_is_in_progress_when_status_starts_with_in_progress(string status, bool expected)
    {
        Assert.Equal(expected, new Project("T", "t.md", status, "", 0).IsInProgress);
    }
}
