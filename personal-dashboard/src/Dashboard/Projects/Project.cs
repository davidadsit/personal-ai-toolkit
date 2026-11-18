using System.Text.RegularExpressions;

namespace PersonalDashboard.Projects;

public sealed partial record Project(string Title, string File, string Status, string NextDate, int Position)
{
    public bool IsDone => Status.StartsWith("Done", StringComparison.Ordinal);

    public bool IsInProgress => Status.StartsWith("In progress", StringComparison.Ordinal);

    public DateOnly? DueDate
    {
        get
        {
            var match = IsoDate().Match(NextDate);
            return match.Success ? DateOnly.ParseExact(match.Value, "yyyy-MM-dd") : null;
        }
    }

    [GeneratedRegex(@"\d{4}-\d{2}-\d{2}")]
    private static partial Regex IsoDate();
}
