namespace PersonalDashboard.Server;

public sealed record ArchiveQueueFile(IReadOnlyList<QueuedEmail> Emails);
