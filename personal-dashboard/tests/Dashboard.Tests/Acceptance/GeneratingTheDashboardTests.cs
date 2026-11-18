using System.Text.RegularExpressions;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Acceptance;

public sealed class GeneratingTheDashboardTests : IDisposable
{
    private static readonly DateTimeOffset WednesdayMorning = new(2026, 11, 18, 8, 5, 0, TimeSpan.FromHours(-7));
    private readonly string root = Directory.CreateTempSubdirectory("dashboard-acceptance-").FullName;
    private WorkspacePaths Paths => new(root);

    public GeneratingTheDashboardTests()
    {
        WriteFile("dashboard.json", """{ "timeZone": "America/Denver", "timeZoneLabel": "MT" }""");
        WriteFile("projects/README.md", """
            | Project | Horizon | Status | Partners | Next date | Outcome | Notes |
            |---|---|---|---|---|---|---|
            | [P1. Kitchen remodel](kitchen.md) | Short | In progress | Contractor | 2027-03-01 | | |
            | [P2. Learn Rust](rust.md) | Long | Planned | | Unknown | | |
            | [P3. Tax return](taxes.md) | Short | Done | | | Filed | |
            """);
        WriteFile("projects/kitchen.md", """
            # Kitchen
            ## Plan
            - [x] P1.1. Measure the room. (done 2026-11-17)
            - [ ] P1.2. **Now:** Get three quotes
            - [ ] P1.3. Order cabinets
            - [ ] P1.4. Install cabinets
            ## Notes
            """);
        WriteFile("projects/rust.md", "## Plan\n- [ ] P2.1. Read the book\n");
        WriteFile("projects/taxes.md", "## Plan\n- [x] P3.1. File the return (done 2026-04-10)\n");
        WriteFile("dashboard/meetings.json", """{ "date": "2026-11-18", "refreshed": "2026-11-18 08:00", "meetings": [ { "start": "09:00", "end": "09:30", "title": "Planning", "critical": true, "reason": "I run this meeting" } ] }""");
        WriteFile("dashboard/emails.json", """{ "refreshed": "2026-11-18 08:00", "emails": [ { "from": "Alex Example", "subject": "Quote for cabinets", "received": "2026-11-16", "why": "Approve the quote by Friday", "link": "https://mail.google.com/mail/u/0/#inbox/abc123", "thread": "abc123" } ] }""");
        WriteFile("dashboard/open-items.json", """{ "items": [ { "code": "A1", "kind": "action", "text": "Book the **plumber**", "raised": "2026-11-15" } ] }""");
    }

    public void Dispose() => Directory.Delete(root, recursive: true);

    private void WriteFile(string relativePath, string content)
    {
        var path = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private string GeneratePage()
    {
        new DashboardGenerator(new FixedClock(WednesdayMorning)).Generate(Paths);
        return File.ReadAllText(Paths.Page);
    }

    private static string Lane(string page, string lane) =>
        Regex.Match(page, $"<div class=\"lane\" data-lane=\"{lane}\".*?(?=<div class=\"lane\"|</section>)", RegexOptions.Singleline).Value;

    [Fact]
    public void Board_shows_what_is_underway_next_and_recently_finished()
    {
        var page = GeneratePage();

        Assert.Contains("Get three quotes", Lane(page, "doing"));
        Assert.Contains("Order cabinets", Lane(page, "todo"));
        Assert.Contains("Install cabinets", Lane(page, "backlog"));
        Assert.Contains("Read the book", Lane(page, "todo"));
        Assert.Contains("Measure the room.", Lane(page, "done"));
        Assert.Contains("File the return", Lane(page, "archive"));
    }

    [Fact]
    public void Finished_projects_are_listed_as_completed()
    {
        Assert.Contains("Completed projects <span class=\"pill\">1</span>", GeneratePage());
    }

    [Fact]
    public void Panels_show_meetings_emails_and_open_items()
    {
        var page = GeneratePage();

        Assert.Contains("9:00 AM–9:30 AM", page);
        Assert.Contains("I run this meeting", page);
        Assert.Contains("Quote for cabinets", page);
        Assert.Contains("Book the <strong>plumber</strong>", page);
    }

    [Fact]
    public void Generating_twice_with_the_same_inputs_and_clock_gives_the_same_page()
    {
        Assert.Equal(GeneratePage(), GeneratePage());
    }

    [Fact]
    public void Creates_the_dashboard_folder_when_missing()
    {
        Directory.Delete(Paths.Output, recursive: true);

        GeneratePage();

        Assert.True(File.Exists(Paths.Page));
    }
}
