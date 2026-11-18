using PersonalDashboard.Feeds;
using PersonalDashboard.Server;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Server;

public sealed class CompletedArchiveTests : IDisposable
{
    private readonly WorkspacePaths paths = new(Directory.CreateTempSubdirectory("completed-archive-").FullName);

    public void Dispose() => Directory.Delete(paths.Root, recursive: true);

    [Fact]
    public void Removes_the_thread_from_the_queue_and_the_email_feed()
    {
        Directory.CreateDirectory(paths.Output);
        File.WriteAllText(paths.Emails, """{ "refreshed": "r", "emails": [ { "from": "A", "subject": "One", "received": "d", "why": "w", "link": "", "thread": "t1" }, { "from": "B", "subject": "Two", "received": "d", "why": "w", "link": "", "thread": "t2" } ] }""");
        var queue = new ArchiveQueue(paths.ArchiveQueue, paths.ArchiveQueueLock);
        queue.Add(new QueuedEmail("t1", "One"));

        new CompletedArchive(paths).Complete("t1");

        Assert.Empty(queue.Read());
        var emails = FeedJson.ReadIfExists<EmailsFeed>(paths.Emails)!;
        Assert.Equal(["t2"], emails.Emails.Select(e => e.Thread));
        Assert.Equal("r", emails.Refreshed);
    }

    [Fact]
    public void Works_when_there_is_no_email_feed()
    {
        Directory.CreateDirectory(paths.Output);

        new CompletedArchive(paths).Complete("t1");

        Assert.False(File.Exists(paths.Emails));
    }
}
