using PersonalDashboard.Server;

namespace PersonalDashboard.Tests.Server;

public sealed class ArchiveQueueTests : IDisposable
{
    private readonly string folder = Directory.CreateTempSubdirectory("archive-queue-").FullName;

    public void Dispose() => Directory.Delete(folder, recursive: true);

    private ArchiveQueue Queue() => new(Path.Combine(folder, "archive-queue.json"), Path.Combine(folder, "archive-queue.lock"));

    [Fact]
    public void Adds_a_thread_once_even_when_added_twice()
    {
        var queue = Queue();

        queue.Add(new QueuedEmail("t1", "Quote"));
        queue.Add(new QueuedEmail("t1", "Quote"));

        Assert.Equal([new QueuedEmail("t1", "Quote")], queue.Read());
    }

    [Fact]
    public void Missing_queue_file_reads_as_empty()
    {
        Assert.Empty(Queue().Read());
    }

    [Fact]
    public void Releases_the_lock_after_adding()
    {
        Queue().Add(new QueuedEmail("t1", "Quote"));

        Assert.False(File.Exists(Path.Combine(folder, "archive-queue.lock")));
    }

    [Fact]
    public void Waits_for_another_writer_to_release_the_lock()
    {
        var lockPath = Path.Combine(folder, "archive-queue.lock");
        File.WriteAllText(lockPath, "");
        _ = Task.Run(async () => { await Task.Delay(300); File.Delete(lockPath); });

        Queue().Add(new QueuedEmail("t1", "Quote"));

        Assert.Single(Queue().Read());
    }

    [Fact]
    public void Removes_a_stale_lock()
    {
        var lockPath = Path.Combine(folder, "archive-queue.lock");
        File.WriteAllText(lockPath, "");
        File.SetLastWriteTimeUtc(lockPath, DateTime.UtcNow.AddMinutes(-1));

        Queue().Add(new QueuedEmail("t1", "Quote"));

        Assert.Single(Queue().Read());
    }

    [Fact]
    public void Removes_a_thread_and_keeps_the_others()
    {
        var queue = Queue();
        queue.Add(new QueuedEmail("t1", "One"));
        queue.Add(new QueuedEmail("t2", "Two"));

        queue.Remove("t1");

        Assert.Equal([new QueuedEmail("t2", "Two")], queue.Read());
    }
}
