using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Workspace;

public sealed class WorkspaceReaderTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("dashboard-test-").FullName;

    public void Dispose() => Directory.Delete(root, recursive: true);

    private void WriteFile(string relativePath, string content)
    {
        var path = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    [Fact]
    public void Empty_workspace_reads_default_settings_and_no_feeds()
    {
        var snapshot = new WorkspaceReader(root).Read();

        Assert.Equal(8771, snapshot.Settings.Port);
        Assert.Equal("default", snapshot.Settings.Theme);
        Assert.Empty(snapshot.Plans);
        Assert.Null(snapshot.Meetings);
        Assert.Null(snapshot.Emails);
        Assert.Null(snapshot.OpenItems);
    }

    [Fact]
    public void Reads_settings_and_ignores_skill_rules()
    {
        WriteFile("dashboard.json", """{ "timeZone": "America/New_York", "timeZoneLabel": "ET", "port": 9000, "theme": "autumn", "keyPeople": ["Sam"] }""");

        var settings = new WorkspaceReader(root).Read().Settings;

        Assert.Equal("America/New_York", settings.TimeZone);
        Assert.Equal("ET", settings.TimeZoneLabel);
        Assert.Equal(9000, settings.Port);
        Assert.Equal(15, settings.ReloadSeconds);
        Assert.Equal("autumn", settings.Theme);
    }

    [Fact]
    public void Reads_each_project_with_its_plan_steps()
    {
        WriteFile("projects/README.md", "| [P1. Kitchen](kitchen.md) | | In progress | | |\n| [P2. Missing](missing.md) | | Planned | | |\n");
        WriteFile("projects/kitchen.md", "## Plan\n- [ ] P1.1. Measure\n");

        var plans = new WorkspaceReader(root).Read().Plans;

        Assert.Equal(["Measure"], plans[0].Steps.Select(s => s.Text));
        Assert.Empty(plans[1].Steps);
    }

    [Fact]
    public void Reads_the_three_feeds()
    {
        WriteFile("dashboard/meetings.json", """{ "date": "2026-11-17", "refreshed": "2026-11-17 08:05", "meetings": [ { "start": "09:00", "end": "09:30", "title": "Planning", "critical": true, "reason": "I run it" } ] }""");
        WriteFile("dashboard/emails.json", """{ "refreshed": "2026-11-17 08:05", "emails": [ { "from": "Alex", "subject": "Quote", "received": "2026-11-16", "why": "Approve", "link": "https://mail.google.com/x", "thread": "abc" } ] }""");
        WriteFile("dashboard/open-items.json", """{ "items": [ { "code": "A1", "kind": "action", "text": "Book", "raised": "2026-11-15", "project": "P1", "folder": "garden-shed" }, { "code": "Q1", "kind": "question", "text": "Why", "raised": "2026-11-16" } ] }""");

        var snapshot = new WorkspaceReader(root).Read();

        Assert.True(snapshot.Meetings!.Meetings.Single().Critical);
        Assert.Equal("abc", snapshot.Emails!.Emails.Single().Thread);
        Assert.Equal(["P1", null], snapshot.OpenItems!.Items.Select(item => item.Project));
        Assert.Equal(["garden-shed", null], snapshot.OpenItems!.Items.Select(item => item.Folder));
    }

    [Fact]
    public void Reads_the_feed_of_each_declared_list_panel()
    {
        WriteFile("dashboard.json", """{ "panels": [ { "feed": "parcels", "title": "Parcels", "instructions": "Find shipping mail." }, { "feed": "library-holds", "title": "Library holds" } ] }""");
        WriteFile("dashboard/parcels.json", """{ "refreshed": "2026-11-17 08:05", "rows": [ { "title": "Garden hose", "date": "2026-11-20" } ] }""");

        var panels = new WorkspaceReader(root).Read().ListPanels;

        Assert.Equal(["Parcels", "Library holds"], panels.Select(panel => panel.Settings.Title));
        var row = panels[0].Feed!.Rows.Single();
        Assert.Equal(("Garden hose", "2026-11-20", "", ""), (row.Title, row.Date, row.Subtitle, row.Link));
        Assert.Null(panels[1].Feed);
    }

    [Fact]
    public void Reads_the_saved_icon_of_each_row_vendor_as_a_data_uri()
    {
        WriteFile("dashboard.json", """{ "panels": [ { "feed": "parcels", "title": "Parcels" } ] }""");
        WriteFile("dashboard/parcels.json", """{ "refreshed": "2026-11-17 08:05", "rows": [ { "title": "Hose", "icon": "example.com" }, { "title": "Rake", "icon": "unsaved.example" }, { "title": "Trap", "icon": "../secrets" } ] }""");
        WriteFile("dashboard/icons/example.com.png", "png bytes");
        WriteFile("secrets.png", "private");

        var icons = new WorkspaceReader(root).Read().ListPanels.Single().Icons;

        Assert.Equal("data:image/png;base64," + Convert.ToBase64String("png bytes"u8.ToArray()), icons["example.com"]);
        Assert.Single(icons);
    }

    [Theory]
    [InlineData("Parcels")]
    [InlineData("../secrets")]
    [InlineData("")]
    [InlineData("emails")]
    [InlineData("archive-queue")]
    public void Rejects_an_invalid_or_reserved_feed_name(string feed)
    {
        WriteFile("dashboard.json", $$"""{ "panels": [ { "feed": "{{feed}}", "title": "Panel" } ] }""");

        var error = Assert.Throws<InvalidOperationException>(() => new WorkspaceReader(root).Read());

        Assert.Contains($"'{feed}'", error.Message);
    }

    [Fact]
    public void Rejects_a_feed_declared_twice()
    {
        WriteFile("dashboard.json", """{ "panels": [ { "feed": "parcels", "title": "A" }, { "feed": "parcels", "title": "B" } ] }""");

        var error = Assert.Throws<InvalidOperationException>(() => new WorkspaceReader(root).Read());

        Assert.Contains("'parcels'", error.Message);
    }

    [Fact]
    public void Finds_the_root_from_a_nested_folder()
    {
        WriteFile("dashboard.json", "{}");
        var nested = Directory.CreateDirectory(Path.Combine(root, "projects", "deep")).FullName;

        Assert.Equal(root, WorkspaceLocator.Find(nested));
    }

    [Fact]
    public void Fails_with_a_message_when_no_root_is_found()
    {
        var error = Assert.Throws<DirectoryNotFoundException>(() => WorkspaceLocator.Find(Path.GetPathRoot(root)!));

        Assert.Contains("dashboard.json", error.Message);
    }
}
