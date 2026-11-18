using PersonalDashboard.Feeds;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class EmailsSection
{
    private const string GmailPrefix = "https://mail.google.com/";

    public static string Render(EmailsFeed? feed) => $"""
        <section id="emails">
        <h2 class="label">Emails to address</h2>
        {Body(feed)}
        </section>
        """;

    private static string Body(EmailsFeed? feed)
    {
        if (feed is null)
        {
            return """<p class="muted">No data yet. Run /dashboard.</p>""";
        }
        var refreshed = $"""<p class="refreshed">Refreshed {Encode(RefreshedTime.ToDisplay(feed.Refreshed))}</p>""";
        if (feed.Emails.Count == 0)
        {
            return $"""{refreshed}<p>Nothing needs you.</p>""";
        }
        return $"""{refreshed}<ul>{string.Concat(feed.Emails.Select(RenderEmail))}</ul><p id="nothing-else" class="muted" hidden>Nothing else needs you.</p>""";
    }

    private static string RenderEmail(Email email)
    {
        var subject = email.Link.StartsWith(GmailPrefix, StringComparison.Ordinal)
            ? $"""<a href="{Encode(email.Link)}" target="_blank" rel="noopener">{Encode(email.Subject)}</a>"""
            : Encode(email.Subject);
        var key = email.Link != "" ? email.Link : email.Subject;
        var archive = email.Thread != "" ? """<button class="archive">Archive</button>""" : "";
        return $"""
            <li class="email" data-key="{Encode(key)}" data-thread="{Encode(email.Thread)}">
            <div class="email-body">
            <div class="subject">{subject}</div>
            <div class="muted">{Encode(email.From)} · {Encode(email.Received)}</div>
            <div class="why">{Encode(email.Why)}</div>
            </div>
            <div class="email-actions"><button class="addressed" title="Addressed">✓</button>{archive}</div>
            </li>
            """;
    }
}
