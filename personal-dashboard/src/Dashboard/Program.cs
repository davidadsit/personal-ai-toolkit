using System.Runtime.InteropServices;
using PersonalDashboard.Server;
using PersonalDashboard.Weather;
using PersonalDashboard.Workspace;

var paths = new WorkspacePaths(WorkspaceLocator.Find(Directory.GetCurrentDirectory()));

switch (args)
{
    case []:
        Generate();
        return 0;
    case ["serve"]:
        Generate();
        await Serve();
        return 0;
    case ["weather"]:
        return await RefreshWeather();
    case ["archive-done", var thread]:
        new CompletedArchive(paths).Complete(thread);
        return 0;
    default:
        Console.Error.WriteLine("Usage: dashboard [serve | weather | archive-done <thread>]");
        return 2;
}

async Task<int> RefreshWeather()
{
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    try
    {
        await new WeatherRefresh(new OpenMeteoClient(http), TimeProvider.System).Refresh(paths);
        return 0;
    }
    catch (Exception error) when (error is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException or KeyNotFoundException)
    {
        Console.Error.WriteLine($"Weather refresh failed, keeping the previous weather: {error.Message}");
        return 1;
    }
}

void Generate() => new DashboardGenerator(TimeProvider.System).Generate(paths);

async Task Serve()
{
    var settings = new WorkspaceReader(paths.Root).Read().Settings;
    using var server = new DashboardServer(paths, settings.Port);
    server.Start();
    File.WriteAllText(paths.ServerPid, Environment.ProcessId.ToString());
    Console.WriteLine($"Serving the dashboard on http://127.0.0.1:{settings.Port}/");

    var stopped = new TaskCompletionSource();
    void Stop(PosixSignalContext context)
    {
        context.Cancel = true;
        stopped.TrySetResult();
    }
    using var interrupt = PosixSignalRegistration.Create(PosixSignal.SIGINT, Stop);
    using var terminate = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Stop);
    await stopped.Task;
}
