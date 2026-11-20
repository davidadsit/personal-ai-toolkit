using System.Text.RegularExpressions;

namespace PersonalDashboard.Workspace;

public static partial class FeedNames
{
    private static readonly string[] Reserved = ["meetings", "emails", "open-items", "archive-queue"];

    public static void EnsureValid(IEnumerable<ListPanelSettings> panels)
    {
        var declared = new HashSet<string>();
        foreach (var feed in panels.Select(panel => panel.Feed ?? ""))
        {
            if (!Pattern().IsMatch(feed) || Reserved.Contains(feed))
            {
                throw new InvalidOperationException($"Invalid list panel feed '{feed}'. Use lowercase letters, digits and dashes, starting with a letter, and not one of: {string.Join(", ", Reserved)}.");
            }
            if (!declared.Add(feed))
            {
                throw new InvalidOperationException($"List panel feed '{feed}' is declared twice.");
            }
        }
    }

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex Pattern();
}
