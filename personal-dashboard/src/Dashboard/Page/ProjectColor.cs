namespace PersonalDashboard.Page;

public static class ProjectColor
{
    private const int HuesPerTheme = 10;

    public static string HueVariable(int position) => $"var(--project-hue-{position % HuesPerTheme})";

    public static string Hue(int position) => $"--hue:{HueVariable(position)}";
}
