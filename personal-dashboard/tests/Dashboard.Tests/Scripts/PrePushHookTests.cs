using System.Diagnostics;

namespace PersonalDashboard.Tests.Scripts;

public sealed class PrePushHookTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("pre-push-").FullName;
    private string Remote => Path.Combine(root, "remote.git");
    private string Clone => Path.Combine(root, "clone");
    private string PrivateWorkspace => Path.Combine(root, "private");

    public PrePushHookTests()
    {
        Directory.CreateDirectory(PrivateWorkspace);
        File.WriteAllText(Path.Combine(PrivateWorkspace, "dashboard.json"), """{ "keyPeople": ["Pat Example"] }""");
        File.WriteAllText(Path.Combine(PrivateWorkspace, "public-denylist.txt"), "# private places\nSecret Ranch\n");
        Git(root, "init", "-q", "--bare", Remote);
        Git(root, "init", "-q", "-b", "main", Clone);
        Git(Clone, "config", "user.email", "tester@example.invalid");
        Git(Clone, "config", "user.name", "Tester");
        Git(Clone, "config", "core.hooksPath", Path.Combine(ToolkitFolder(), ".githooks"));
        Git(Clone, "remote", "add", "origin", Remote);
    }

    public void Dispose() => Directory.Delete(root, recursive: true);

    private static string ToolkitFolder()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, ".githooks", "pre-push")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException(".githooks/pre-push was not found above the test assembly.");
    }

    private static (int ExitCode, string Errors) Git(string folder, params string[] arguments)
    {
        var start = new ProcessStartInfo("git") { WorkingDirectory = folder, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        using var git = Process.Start(start)!;
        git.StandardOutput.ReadToEnd();
        var errors = git.StandardError.ReadToEnd();
        git.WaitForExit();
        return (git.ExitCode, errors);
    }

    private (int ExitCode, string Errors) CommitAndPush(string content)
    {
        File.WriteAllText(Path.Combine(Clone, "notes.md"), content);
        Git(Clone, "add", "notes.md");
        Git(Clone, "commit", "-q", "-m", "Add notes");
        return Git(Clone, "push", "-q", "origin", "main");
    }

    private void UsePrivateWorkspace() => Git(Clone, "config", "personal-ai.private-workspace", PrivateWorkspace);

    [Fact]
    public void Allows_a_push_without_private_terms()
    {
        UsePrivateWorkspace();

        var push = CommitAndPush("Build a garden shed\n");

        Assert.True(push.ExitCode == 0, push.Errors);
    }

    [Fact]
    public void Blocks_a_push_that_names_a_key_person()
    {
        UsePrivateWorkspace();

        var push = CommitAndPush("Ask pat example about the plan\n");

        Assert.NotEqual(0, push.ExitCode);
        Assert.Contains("pat example", push.Errors);
    }

    [Fact]
    public void Blocks_a_push_that_contains_a_denylisted_term()
    {
        UsePrivateWorkspace();

        var push = CommitAndPush("Fix the signs at Secret Ranch\n");

        Assert.NotEqual(0, push.ExitCode);
        Assert.Contains("Secret Ranch", push.Errors);
    }

    [Fact]
    public void Matches_whole_words_only()
    {
        UsePrivateWorkspace();
        File.WriteAllText(Path.Combine(PrivateWorkspace, "public-denylist.txt"), "Rob\n");

        var push = CommitAndPush("A robust and simple design\n");

        Assert.True(push.ExitCode == 0, push.Errors);
    }

    [Fact]
    public void Blocks_a_push_when_no_private_workspace_is_configured()
    {
        var push = CommitAndPush("Build a garden shed\n");

        Assert.NotEqual(0, push.ExitCode);
        Assert.Contains("personal-ai.private-workspace", push.Errors);
    }
}
