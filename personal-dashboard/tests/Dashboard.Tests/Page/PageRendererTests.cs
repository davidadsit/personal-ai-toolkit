using System.Text.RegularExpressions;
using PersonalDashboard.Feeds;
using PersonalDashboard.Kanban;
using PersonalDashboard.Page;
using PersonalDashboard.Projects;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Page;

public class PageRendererTests
{
    private static readonly DateTimeOffset GeneratedAt = new(2026, 11, 18, 8, 5, 0, TimeSpan.FromHours(-7));
    private static readonly DateOnly Today = new(2026, 11, 18);
    private static readonly BoardBuilder Builder = new(new LaneAssigner(2, 2));

    private static ProjectPlan Plan(int position, string status, params string[] openSteps) =>
        new(new Project($"P{position + 1}. Project {position + 1}", $"p{position}.md", status, "", position),
            openSteps.Select((text, order) => new Step($"P{position + 1}.{order + 1}", text, false, null, false, order)).ToList());

    private static string Render(
        IReadOnlyList<ProjectPlan>? plans = null,
        MeetingsFeed? meetings = null,
        EmailsFeed? emails = null,
        OpenItemsFeed? openItems = null,
        string theme = "default",
        IReadOnlyList<ListPanel>? listPanels = null)
    {
        plans ??= [];
        var snapshot = new WorkspaceSnapshot(new DashboardSettings { Theme = theme }, plans, meetings, emails, openItems, listPanels ?? []);
        return PageRenderer.Render(new PageContent(snapshot, Builder.Build(plans, Today), GeneratedAt));
    }

    private static string Section(string html, string id) =>
        Regex.Match(html, $"<section id=\"{id}\".*?</section>", RegexOptions.Singleline).Value;

    private static ListPanel Parcels(params ListPanelRow[] rows) =>
        new(new ListPanelSettings("parcels", "Parcels"), new ListPanelFeed("2026-11-18 08:00", rows));

    private static string RenderParcels(params ListPanelRow[] rows) =>
        Section(Render(listPanels: [Parcels(rows)]), "panel-parcels");

    [Fact]
    public void List_panels_render_between_meetings_and_emails_in_declared_order()
    {
        var holds = new ListPanel(new ListPanelSettings("library-holds", "Library holds"), null);

        var page = Render(listPanels: [Parcels(), holds]);

        var order = new[] { "id=\"meetings\"", "id=\"panel-parcels\"", "id=\"panel-library-holds\"", "id=\"emails\"" }
            .Select(marker => page.IndexOf(marker, StringComparison.Ordinal)).ToList();
        Assert.DoesNotContain(-1, order);
        Assert.Equal(order.Order(), order);
        Assert.Contains("<h2 class=\"label\">Library holds</h2>", page);
    }

    [Fact]
    public void List_panel_without_a_feed_asks_for_a_refresh()
    {
        var page = Render(listPanels: [new ListPanel(new ListPanelSettings("parcels", "Parcels"), null)]);

        Assert.Contains("No data yet. Run /dashboard.", Section(page, "panel-parcels"));
    }

    [Fact]
    public void List_panel_with_no_rows_shows_the_refresh_time_and_nothing_to_show()
    {
        var panel = RenderParcels();

        Assert.Contains("Refreshed 2026-11-18 8:00 AM", panel);
        Assert.Contains("Nothing to show.", panel);
    }

    [Fact]
    public void List_panel_rows_sort_by_date_then_time_with_undated_rows_last()
    {
        var panel = RenderParcels(
            new ListPanelRow("Undated first in file"),
            new ListPanelRow("Later day", Date: "2026-11-24"),
            new ListPanelRow("Same day afternoon", Date: "2026-11-20", Time: "14:00"),
            new ListPanelRow("Undated second in file", Date: "soon"),
            new ListPanelRow("Same day morning", Date: "2026-11-20", Time: "09:00"));

        var titles = Regex.Matches(panel, "<div class=\"row-title\">(.*?)</div>").Select(match => match.Groups[1].Value);
        Assert.Equal(["Same day morning", "Same day afternoon", "Later day", "Undated first in file", "Undated second in file"], titles);
    }

    [Fact]
    public void List_panel_row_shows_subtitle_status_and_short_date_with_time()
    {
        var panel = RenderParcels(new ListPanelRow("Garden hose", "Hardware store", "**Shipped**", "2026-11-24", "14:00"));

        Assert.Contains("<div class=\"muted\">Hardware store</div>", panel);
        Assert.Contains("<div class=\"row-status\"><strong>Shipped</strong></div>", panel);
        Assert.Contains("<div class=\"row-when\">Tue 11/24 2:00 PM</div>", panel);
    }

    [Fact]
    public void List_panel_row_title_is_a_link_only_for_https()
    {
        var panel = RenderParcels(
            new ListPanelRow("Tracked", Link: "https://example.com/track/1"),
            new ListPanelRow("Untracked", Link: "javascript:alert(1)"));

        Assert.Contains("<a href=\"https://example.com/track/1\" target=\"_blank\" rel=\"noopener\">Tracked</a>", panel);
        Assert.Contains("<div class=\"row-title\">Untracked</div>", panel);
        Assert.DoesNotContain("javascript:", panel);
    }

    [Fact]
    public void Header_shows_generation_time_and_reload_interval()
    {
        Assert.Contains("Generated 2026-11-18 8:05 AM MT. It reloads every 15 seconds.", Render());
    }

    [Fact]
    public void Page_has_the_board_favicon_as_an_svg_data_uri()
    {
        var link = Regex.Match(Render(), "<link rel=\"icon\" type=\"image/svg\\+xml\" href=\"data:image/svg\\+xml;base64,([^\"]+)\">");

        var svg = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(link.Groups[1].Value));
        Assert.StartsWith("<svg", svg);
        Assert.Contains("fill=\"#f2b84b\"", svg);
    }

    [Fact]
    public void Project_list_has_all_button_and_one_button_per_open_project()
    {
        var projects = Section(Render([Plan(0, "In progress", "a"), Plan(1, "Done")]), "projects");

        Assert.Contains(">All<", projects);
        Assert.Contains("P1. Project 1", projects);
        Assert.Contains("In progress", projects);
    }

    [Fact]
    public void Done_projects_move_to_completed_projects_with_a_check_mark()
    {
        var projects = Section(Render([Plan(0, "In progress", "a"), Plan(1, "Done")]), "projects");

        Assert.Contains("Completed projects <span class=\"pill\">1</span>", projects);
        Assert.Contains("✓ P2. Project 2", projects);
    }

    [Fact]
    public void Completed_projects_are_hidden_when_none_are_done()
    {
        Assert.DoesNotContain("Completed projects", Render([Plan(0, "Planned", "a")]));
    }

    [Fact]
    public void Project_button_shows_status_and_next_date_with_markdown()
    {
        var plan = new ProjectPlan(new Project("P1", "p.md", "**Blocked**", "2026-12-01", 0), []);

        Assert.Contains("<strong>Blocked</strong> · 2026-12-01", Render([plan]));
    }

    [Fact]
    public void Board_has_five_lanes_in_order()
    {
        var lanes = Regex.Matches(Section(Render(), "board"), "data-lane=\"(\\w+)\"").Select(m => m.Groups[1].Value);

        Assert.Equal(["backlog", "todo", "doing", "done", "archive"], lanes);
    }

    [Fact]
    public void Stacked_card_lists_both_projects_and_ids_and_belongs_to_both()
    {
        var board = Section(Render([Plan(0, "Planned", "Call the bank"), Plan(1, "Planned", "Call the bank")]), "board");

        Assert.Contains("data-projects=\"0 1\"", board);
        Assert.Contains("P1.1 · P2.1", board);
        Assert.Contains("P1. Project 1 · P2. Project 2", board);
    }

    [Fact]
    public void Card_tint_uses_the_hue_of_its_project_position()
    {
        var board = Section(Render([Plan(0, "Planned", "a"), Plan(1, "Planned", "b")]), "board");

        Assert.Contains("--hue:var(--project-hue-0)", board);
        Assert.Contains("--hue:var(--project-hue-1)", board);
    }

    [Fact]
    public void Missing_meetings_feed_asks_for_a_refresh()
    {
        Assert.Contains("No data yet. Run /dashboard.", Section(Render(), "meetings"));
    }

    [Fact]
    public void Meetings_feed_from_yesterday_is_not_refreshed_today()
    {
        var meetings = Section(Render(meetings: new MeetingsFeed(Today.AddDays(-1), "2026-11-17 08:05", [])), "meetings");

        Assert.Contains("Refreshed 2026-11-17 8:05 AM", meetings);
        Assert.Contains("Not refreshed today. Run /dashboard.", meetings);
    }

    [Fact]
    public void Day_without_meetings_says_so()
    {
        Assert.Contains("No meetings today.", Section(Render(meetings: new MeetingsFeed(Today, "x", [])), "meetings"));
    }

    [Fact]
    public void Critical_meeting_shows_badge_reason_and_twelve_hour_times()
    {
        var feed = new MeetingsFeed(Today, "x", [new Meeting("09:00", "13:30", "Planning", true, "I run this")]);

        var meetings = Section(Render(meetings: feed), "meetings");

        Assert.Contains("9:00 AM–1:30 PM", meetings);
        Assert.Contains("Critical", meetings);
        Assert.Contains("I run this", meetings);
    }

    [Fact]
    public void Meeting_carries_start_and_end_instants_for_the_browser()
    {
        var feed = new MeetingsFeed(Today, "x", [new Meeting("09:00", "09:30", "Planning", false, "")]);

        var meetings = Section(Render(meetings: feed), "meetings");

        Assert.Contains($"data-start=\"{new DateTimeOffset(2026, 11, 18, 16, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds()}\"", meetings);
    }

    [Fact]
    public void Missing_emails_feed_asks_for_a_refresh()
    {
        Assert.Contains("No data yet. Run /dashboard.", Section(Render(), "emails"));
    }

    [Fact]
    public void Empty_email_list_says_nothing_needs_you()
    {
        var emails = Section(Render(emails: new EmailsFeed("2026-11-18 08:05", [])), "emails");

        Assert.Contains("Refreshed 2026-11-18 8:05 AM", emails);
        Assert.Contains("Nothing needs you.", emails);
    }

    [Fact]
    public void Email_with_gmail_link_links_its_subject_and_offers_archive()
    {
        var feed = new EmailsFeed("x", [new Email("Alex", "Quote", "2026-11-16", "Approve", "https://mail.google.com/mail/u/0/#inbox/abc", "abc")]);

        var emails = Section(Render(emails: feed), "emails");

        Assert.Contains("<a href=\"https://mail.google.com/mail/u/0/#inbox/abc\"", emails);
        Assert.Contains("class=\"archive\"", emails);
        Assert.Contains("Alex · 2026-11-16", emails);
    }

    [Fact]
    public void Email_without_gmail_link_or_thread_is_plain_text_without_archive()
    {
        var feed = new EmailsFeed("x", [new Email("Alex", "Quote", "2026-11-16", "Approve", "https://evil.example/", "")]);

        var emails = Section(Render(emails: feed), "emails");

        Assert.DoesNotContain("<a ", emails);
        Assert.DoesNotContain("class=\"archive\"", emails);
    }

    [Fact]
    public void Open_items_render_oldest_first_with_markdown_and_encoding()
    {
        var feed = new OpenItemsFeed([
            new OpenItem("Q1", "question", "Gas <or> induction?", new DateOnly(2026, 11, 15)),
            new OpenItem("A1", "action", "Book the **plumber** for `2026-12-01`", new DateOnly(2026, 11, 10)),
        ]);

        var items = Section(Render(openItems: feed), "open-items");

        Assert.True(items.IndexOf("A1", StringComparison.Ordinal) < items.IndexOf("Q1", StringComparison.Ordinal));
        Assert.Contains("Book the <strong>plumber</strong> for <code>2026-12-01</code>", items);
        Assert.Contains("Gas &lt;or&gt; induction?", items);
        Assert.Contains("Raised 2026-11-10", items);
    }

    [Fact]
    public void Missing_open_items_say_nothing_open()
    {
        Assert.Contains("Nothing open.", Section(Render(), "open-items"));
    }

    [Theory]
    [InlineData("default")]
    [InlineData("autumn")]
    [InlineData("christmas")]
    public void Page_references_no_network_address_except_data_links_and_the_calendar(string theme)
    {
        var feed = new EmailsFeed("x", [new Email("A", "S", "d", "w", "https://mail.google.com/mail/u/0/#inbox/abc", "abc")]);

        var html = Render([Plan(0, "Planned", "a")], emails: feed, theme: theme);

        var addresses = Regex.Matches(html, @"(?:https?:)?//[^\s""'<>)]+")
            .Select(m => m.Value)
            .Where(address => !address.StartsWith(SvgNamespace, StringComparison.Ordinal))
            .Distinct();
        Assert.All(addresses, address => Assert.Contains(address, new[] { "https://calendar.google.com/calendar/r", "https://mail.google.com/mail/u/0/#inbox/abc" }));
    }

    private const string SvgNamespace = "http://www.w3.org/2000/svg";

    [Theory]
    [InlineData("autumn", "--project-hue-0: 24;")]
    [InlineData("christmas", "--project-hue-0: 355;")]
    public void Page_uses_the_configured_theme(string theme, string firstHue)
    {
        Assert.Contains(firstHue, Render(theme: theme));
    }

    [Fact]
    public void Default_theme_keeps_the_original_project_hues()
    {
        Assert.Contains("--project-hue-0: 210;", Render());
    }

    [Fact]
    public void Unknown_theme_fails_with_the_available_themes()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Render(theme: "neon"));

        Assert.Contains("autumn", error.Message);
        Assert.Contains("default", error.Message);
    }

    [Fact]
    public void Project_list_has_a_projects_label()
    {
        Assert.Contains("<h2 class=\"label\">Projects</h2>", Section(Render(), "projects"));
    }

    [Fact]
    public void Meetings_refreshed_today_show_when_they_were_refreshed()
    {
        var meetings = Section(Render(meetings: new MeetingsFeed(Today, "2026-11-18 16:30", [])), "meetings");

        Assert.Contains("Refreshed 2026-11-18 4:30 PM", meetings);
    }

    [Fact]
    public void Refreshed_time_that_is_not_a_timestamp_shows_as_written()
    {
        Assert.Contains("Refreshed soon", Section(Render(emails: new EmailsFeed("soon", [])), "emails"));
    }

    [Fact]
    public void Lane_counts_show_as_pills()
    {
        Assert.Contains("<span class=\"count pill\">", Section(Render(), "board"));
    }

    [Fact]
    public void Open_item_shows_its_project_title_next_to_the_raised_date()
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17), "P2")]);

        var items = Section(Render([Plan(0, "Planned", "a"), Plan(1, "Planned", "b")], openItems: feed), "open-items");

        Assert.Matches("Raised 2026-11-17 <span class=\"item-project tinted\" style=\"--hue:var\\(--project-hue-1\\);--saturation:var\\(--project-saturation-1, 1\\)\">P2. Project 2</span>", items);
    }

    [Fact]
    public void Open_item_with_an_unknown_project_shows_the_project_id()
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17), "P9")]);

        Assert.Contains("Raised 2026-11-17 <span class=\"item-project\">P9</span>", Section(Render(openItems: feed), "open-items"));
    }

    [Fact]
    public void Open_item_without_a_project_shows_no_project()
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17))]);

        Assert.DoesNotContain("item-project", Section(Render(openItems: feed), "open-items"));
    }

    [Fact]
    public void Open_item_shows_the_folder_it_came_from_after_its_project()
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17), "P9", "garden-shed")]);

        Assert.Contains("P9</span> <span class=\"item-folder\">garden-shed</span>", Section(Render(openItems: feed), "open-items"));
    }

    [Fact]
    public void Open_item_without_a_folder_shows_no_folder()
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17), "P9")]);

        Assert.DoesNotContain("item-folder", Section(Render(openItems: feed), "open-items"));
    }

    [Fact]
    public void Open_item_belongs_to_its_project_for_filtering()
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17), "P2")]);

        var items = Section(Render([Plan(0, "Planned", "a"), Plan(1, "Planned", "b")], openItems: feed), "open-items");

        Assert.Contains("<li class=\"open-item\" data-projects=\"1\">", items);
    }

    [Theory]
    [InlineData("P9")]
    [InlineData(null)]
    public void Open_item_without_a_known_project_belongs_to_no_project(string? project)
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17), project)]);

        Assert.Contains("<li class=\"open-item\" data-projects=\"\">", Section(Render(openItems: feed), "open-items"));
    }

    [Fact]
    public void Open_items_have_a_hidden_message_for_a_project_with_none()
    {
        var feed = new OpenItemsFeed([new OpenItem("Q1", "question", "When?", new DateOnly(2026, 11, 17))]);

        Assert.Contains("<p id=\"no-open-items-for-project\" class=\"muted\" hidden>Nothing open for this project.</p>", Section(Render(openItems: feed), "open-items"));
    }

    [Fact]
    public void Card_takes_the_saturation_of_its_project_position()
    {
        var board = Section(Render([Plan(0, "Planned", "a"), Plan(1, "Planned", "b")]), "board");

        Assert.Contains("--saturation:var(--project-saturation-1, 1)", board);
    }

    [Fact]
    public void Autumn_theme_mutes_some_projects_into_browns()
    {
        Assert.Matches(@"--project-saturation-\d: 0\.\d+;", Render(theme: "autumn"));
    }
}
