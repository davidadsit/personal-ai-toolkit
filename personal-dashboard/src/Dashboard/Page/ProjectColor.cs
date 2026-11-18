namespace PersonalDashboard.Page;

public static class ProjectColor
{
    private const int ColorsPerTheme = 10;

    public static string Style(int position) => $"--hue:{HueVariable(position)};--saturation:{SaturationVariable(position)}";

    public static string Edge(int position) =>
        $"hsl({HueVariable(position)} calc(var(--edge-s) * {SaturationVariable(position)}) var(--edge-l))";

    private static string HueVariable(int position) => $"var(--project-hue-{position % ColorsPerTheme})";

    private static string SaturationVariable(int position) => $"var(--project-saturation-{position % ColorsPerTheme}, 1)";
}
