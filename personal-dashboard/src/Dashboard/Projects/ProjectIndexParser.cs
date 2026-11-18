using System.Text.RegularExpressions;

namespace PersonalDashboard.Projects;

public static partial class ProjectIndexParser
{
    public static IReadOnlyList<Project> Parse(string index) =>
        index.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("| [", StringComparison.Ordinal))
            .Select((row, position) => ParseRow(row, position))
            .ToList();

    private static Project ParseRow(string row, int position)
    {
        var cells = row.Split('|').Select(cell => cell.Trim()).ToArray();
        var link = TitleLink().Match(cells[1]);
        return new Project(link.Groups["title"].Value, link.Groups["file"].Value, CellOrEmpty(cells, 3), CellOrEmpty(cells, 5), position);
    }

    private static string CellOrEmpty(string[] cells, int index) => index < cells.Length ? cells[index] : "";

    [GeneratedRegex(@"^\[(?<title>.*)\]\((?<file>[^)]*)\)")]
    private static partial Regex TitleLink();
}
