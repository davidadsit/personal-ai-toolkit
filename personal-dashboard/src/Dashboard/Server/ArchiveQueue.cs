using System.Text.Json;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Server;

public sealed class ArchiveQueue(string queuePath, string lockPath)
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan StaleLockAge = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(50);

    public IReadOnlyList<QueuedEmail> Read() =>
        FeedJson.ReadIfExists<ArchiveQueueFile>(queuePath)?.Emails ?? [];

    public void Add(QueuedEmail email)
    {
        using var heldLock = AcquireLock();
        var emails = Read();
        if (emails.Any(queued => queued.Thread == email.Thread))
        {
            return;
        }
        Write([.. emails, email]);
    }

    public void Remove(string thread)
    {
        using var heldLock = AcquireLock();
        Write(Read().Where(queued => queued.Thread != thread).ToList());
    }

    private void Write(IReadOnlyList<QueuedEmail> emails) =>
        File.WriteAllText(queuePath, JsonSerializer.Serialize(new ArchiveQueueFile(emails), FeedJson.Options));

    private FileStream AcquireLock()
    {
        var deadline = DateTime.UtcNow + LockTimeout;
        while (true)
        {
            try
            {
                return new FileStream(lockPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose);
            }
            catch (IOException) when (File.Exists(lockPath))
            {
                RemoveIfStale();
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException($"The archive queue lock {lockPath} was held for more than {LockTimeout.TotalSeconds} seconds.");
                }
                Thread.Sleep(RetryDelay);
            }
        }
    }

    private void RemoveIfStale()
    {
        if (DateTime.UtcNow - File.GetLastWriteTimeUtc(lockPath) > StaleLockAge)
        {
            File.Delete(lockPath);
        }
    }
}
