using PersonalDashboard.Kanban;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class BoardSection
{
    private static readonly (Lane Lane, string Id, string Heading, bool StartsCollapsed)[] Lanes =
    [
        (Lane.Backlog, "backlog", "Backlog", true),
        (Lane.ToDo, "todo", "To do", false),
        (Lane.Doing, "doing", "Doing", false),
        (Lane.Done, "done", "Done", false),
        (Lane.Archive, "archive", "Archive", true),
    ];

    private const int StackOffsetPixels = 4;

    public static string Render(Board board) =>
        $"""<section id="board" class="board">{string.Concat(Lanes.Select(lane => RenderLane(board, lane)))}</section>""";

    private static string RenderLane(Board board, (Lane Lane, string Id, string Heading, bool StartsCollapsed) lane)
    {
        var cards = board.CardsIn(lane.Lane);
        var heading = $"""{lane.Heading} <span class="count pill">{cards.Count}</span>""";
        var title = lane.StartsCollapsed
            ? $"""<button class="lane-toggle">{heading}</button>"""
            : $"""<h2 class="lane-heading">{heading}</h2>""";
        var collapsible = lane.StartsCollapsed ? " data-collapsible" : "";
        return $"""<div class="lane" data-lane="{lane.Id}"{collapsible}>{title}{string.Concat(cards.Select(RenderCard))}</div>""";
    }

    private static string RenderCard(Card card)
    {
        var finished = card.Lane is Lane.Done or Lane.Archive ? " finished" : "";
        var projects = string.Join(" ", card.Projects.Select(project => project.Position));
        var ids = card.Ids.Count == 0 ? "" : $"<strong>{JoinEncoded(card.Ids)}</strong> ";
        var titles = JoinEncoded(card.Projects.Select(project => project.Title));
        return $"""<div class="card tinted{finished}" data-projects="{projects}" style="{CardStyle(card)}"><div class="card-text">{ids}{ToHtml(card.Text)}</div><div class="card-projects">{titles}</div></div>""";
    }

    private static string JoinEncoded(IEnumerable<string> parts) => string.Join(" · ", parts.Select(Encode));

    private static string CardStyle(Card card)
    {
        var style = ProjectColor.Hue(card.Projects[0].Position);
        var stackedEdges = card.Projects.Skip(1).Select((project, index) =>
            $"{StackOffsetPixels * (index + 1)}px {StackOffsetPixels * (index + 1)}px 0 hsl({ProjectColor.HueVariable(project.Position)} var(--edge-s) var(--edge-l))");
        return card.Projects.Count == 1 ? style : $"{style};box-shadow:{string.Join(",", stackedEdges)}";
    }
}
