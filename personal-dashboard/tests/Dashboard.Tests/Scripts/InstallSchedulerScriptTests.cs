using System.Diagnostics;
using System.Xml.Linq;

namespace PersonalDashboard.Tests.Scripts;

public sealed class InstallSchedulerScriptTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("install-scheduler-").FullName;
    private string Home => Path.Combine(root, "home");
    private string Workspace => Path.Combine(root, "workspace");
    private string FakeTools => Path.Combine(root, "bin");

    public InstallSchedulerScriptTests()
    {
        Directory.CreateDirectory(Path.Combine(Home, "Library", "LaunchAgents"));
        Directory.CreateDirectory(Workspace);
        Directory.CreateDirectory(FakeTools);
        var launchctl = Path.Combine(FakeTools, "launchctl");
        File.WriteAllText(launchctl, "#!/bin/sh\nexit 0\n");
        File.SetUnixFileMode(launchctl, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    public void Dispose() => Directory.Delete(root, recursive: true);

    private static string CodeFolder()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "scripts", "install-scheduler.sh")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("scripts/install-scheduler.sh was not found above the test assembly.");
    }

    private XDocument InstallScheduler()
    {
        var start = new ProcessStartInfo(Path.Combine(CodeFolder(), "scripts", "install-scheduler.sh"))
        {
            WorkingDirectory = Workspace,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        start.ArgumentList.Add(Workspace);
        start.Environment["HOME"] = Home;
        start.Environment["PATH"] = FakeTools + ":/usr/bin:/bin";
        using var script = Process.Start(start)!;
        script.StandardOutput.ReadToEnd();
        var errors = script.StandardError.ReadToEnd();
        script.WaitForExit();
        Assert.True(script.ExitCode == 0, errors);

        var plist = Directory.GetFiles(Path.Combine(Home, "Library", "LaunchAgents"), "*.plist").Single();
        return XDocument.Load(plist);
    }

    private static List<(int Weekday, int Hour, int Minute)> StartTimes(XDocument plist)
    {
        var intervals = plist.Descendants("key").Single(key => key.Value == "StartCalendarInterval").ElementsAfterSelf().First();
        return intervals.Elements("dict").Select(interval =>
        {
            int Value(string name) => int.Parse(interval.Elements("key").Single(key => key.Value == name).ElementsAfterSelf().First().Value);
            return (Value("Weekday"), Value("Hour"), Value("Minute"));
        }).ToList();
    }

    [Fact]
    public void Schedules_a_refresh_every_half_hour_from_8_00_to_17_30_on_weekdays()
    {
        var startTimes = StartTimes(InstallScheduler());

        var expected =
            from weekday in Enumerable.Range(1, 5)
            from hour in Enumerable.Range(8, 10)
            from minute in new[] { 0, 30 }
            select (weekday, hour, minute);
        Assert.Equal(expected.ToList(), startTimes);
    }
}
