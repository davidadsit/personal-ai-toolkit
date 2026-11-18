using System.Text.RegularExpressions;

namespace PersonalDashboard.Projects;

public static partial class PlanParser
{
    private const string PlanHeading = "## Plan";
    private const string NowMarker = "**Now:** ";

    public static IReadOnlyList<Step> Parse(string projectFile) =>
        PlanLines(projectFile)
            .Select(line => StepLine().Match(line.TrimEnd()))
            .Where(match => match.Success)
            .Select((match, order) => ToStep(match, order))
            .ToList();

    private static IEnumerable<string> PlanLines(string projectFile) =>
        projectFile.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .SkipWhile(line => line != PlanHeading)
            .Skip(1)
            .TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal));

    private static Step ToStep(Match match, int order)
    {
        var text = match.Groups["text"].Value;
        var hasNowMarker = text.StartsWith(NowMarker, StringComparison.Ordinal);
        var isFinished = match.Groups["mark"].Value != " ";
        var done = match.Groups["done"];
        return new Step(
            match.Groups["id"].Value,
            hasNowMarker ? text[NowMarker.Length..] : text,
            isFinished,
            done.Success ? DateOnly.ParseExact(done.Value, "yyyy-MM-dd") : null,
            hasNowMarker && !isFinished,
            order);
    }

    [GeneratedRegex(@"^- \[(?<mark>[ xX])\] (?:(?<id>P\d+\.\d+)\. )?(?<text>.*?)(?: \(done (?<done>\d{4}-\d{2}-\d{2})\))?$")]
    private static partial Regex StepLine();
}
