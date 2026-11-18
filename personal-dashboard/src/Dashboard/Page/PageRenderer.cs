using System.Globalization;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class PageRenderer
{
    public static string Render(PageContent content)
    {
        var settings = content.Snapshot.Settings;
        var generated = content.GeneratedAt.ToString("yyyy-MM-dd h:mm tt", CultureInfo.InvariantCulture);
        return $"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>Dashboard</title>
            <style>
            {PageAssets.Theme(settings.Theme)}
            {PageAssets.Styles}
            </style>
            </head>
            <body data-reload-seconds="{settings.ReloadSeconds}">
            <main id="work">
            <h1>Dashboard</h1>
            <p class="generated">Generated {generated} {Encode(settings.TimeZoneLabel)}. It reloads every {settings.ReloadSeconds} seconds.</p>
            {ProjectListSection.Render(content.Board)}
            {BoardSection.Render(content.Board)}
            </main>
            <aside id="panels">
            {MeetingsSection.Render(content.Snapshot.Meetings, settings, content.Today)}
            {EmailsSection.Render(content.Snapshot.Emails)}
            {OpenItemsSection.Render(content.Snapshot.OpenItems, content.Board.Projects)}
            </aside>
            <script>
            {PageAssets.Script}
            </script>
            </body>
            </html>
            """;
    }
}
