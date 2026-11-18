namespace PersonalDashboard.Workspace;

public static class WorkspaceLocator
{
    public static string Find(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "dashboard.json")) || Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException($"No folder holding dashboard.json or .git was found above {startDirectory}.");
    }
}
