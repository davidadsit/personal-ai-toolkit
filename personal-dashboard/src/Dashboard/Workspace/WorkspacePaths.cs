namespace PersonalDashboard.Workspace;

public sealed class WorkspacePaths(string root)
{
    public string Root => root;
    public string Settings => Path.Combine(root, "dashboard.json");
    public string Projects => Path.Combine(root, "projects");
    public string ProjectIndex => Path.Combine(Projects, "README.md");
    public string Output => Path.Combine(root, "dashboard");
    public string Page => Path.Combine(Output, "index.html");
    public string Meetings => Path.Combine(Output, "meetings.json");
    public string Emails => Path.Combine(Output, "emails.json");
    public string OpenItems => Path.Combine(Output, "open-items.json");
    public string ListPanelFeed(string feed) => Path.Combine(Output, feed + ".json");
    public string Weather => Path.Combine(Output, "weather.json");
    public string Icons => Path.Combine(Output, "icons");
    public string ArchiveQueue => Path.Combine(Output, "archive-queue.json");
    public string ArchiveQueueLock => Path.Combine(Output, "archive-queue.lock");
    public string ServerPid => Path.Combine(Output, "server.pid");
    public string Theme(string name) => Path.Combine(root, "themes", name + ".css");
}
