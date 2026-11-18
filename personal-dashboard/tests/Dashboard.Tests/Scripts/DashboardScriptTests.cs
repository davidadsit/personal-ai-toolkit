using System.Diagnostics;

namespace PersonalDashboard.Tests.Scripts;

public sealed class DashboardScriptTests : IDisposable
{
    private readonly string workspace = Directory.CreateTempSubdirectory("dashboard-workspace-").FullName;

    public void Dispose() => Directory.Delete(workspace, recursive: true);

    private static string CodeFolder()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "scripts", "dashboard")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("scripts/dashboard was not found above the test assembly.");
    }

    [Fact]
    public void Generates_the_page_for_the_workspace_it_runs_in_when_the_code_lives_elsewhere()
    {
        File.WriteAllText(Path.Combine(workspace, "dashboard.json"), """{ "timeZone": "America/Denver" }""");
        Directory.CreateDirectory(Path.Combine(workspace, "projects"));
        File.WriteAllText(Path.Combine(workspace, "projects", "README.md"), "| [P1. Outside project](outside.md) | | Planned | | |\n");
        File.WriteAllText(Path.Combine(workspace, "projects", "outside.md"), "## Plan\n- [ ] P1.1. Prove the workspace is separate\n");

        using var script = Process.Start(new ProcessStartInfo(Path.Combine(CodeFolder(), "scripts", "dashboard"))
        {
            WorkingDirectory = workspace,
            RedirectStandardError = true,
        })!;
        var errors = script.StandardError.ReadToEnd();
        script.WaitForExit();

        Assert.True(script.ExitCode == 0, errors);
        Assert.Contains("Prove the workspace is separate", File.ReadAllText(Path.Combine(workspace, "dashboard", "index.html")));
    }
}
