using System.Text.Json;
using PersonalDashboard.Feeds;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Server;

public sealed class CompletedArchive(WorkspacePaths paths)
{
    public void Complete(string thread)
    {
        new ArchiveQueue(paths.ArchiveQueue, paths.ArchiveQueueLock).Remove(thread);

        var feed = FeedJson.ReadIfExists<EmailsFeed>(paths.Emails);
        if (feed is null)
        {
            return;
        }
        var remaining = feed with { Emails = feed.Emails.Where(email => email.Thread != thread).ToList() };
        File.WriteAllText(paths.Emails, JsonSerializer.Serialize(remaining, FeedJson.Options));
    }
}
