using System.Net;
using System.Text.Json;
using PersonalDashboard.Feeds;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Server;

public sealed class DashboardServer(WorkspacePaths paths, int port) : IDisposable
{
    private readonly HttpListener listener = new() { Prefixes = { $"http://127.0.0.1:{port}/" } };
    private readonly ArchiveQueue archiveQueue = new(paths.ArchiveQueue, paths.ArchiveQueueLock);
    private string Origin => $"http://127.0.0.1:{port}";

    public void Start()
    {
        listener.Start();
        _ = Task.Run(AcceptRequests);
    }

    public void Dispose() => listener.Close();

    private async Task AcceptRequests()
    {
        while (listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync();
            }
            catch (Exception exception) when (exception is HttpListenerException or ObjectDisposedException)
            {
                return;
            }
            _ = Task.Run(() => Respond(context));
        }
    }

    private void Respond(HttpListenerContext context)
    {
        using var response = context.Response;
        var request = context.Request;
        switch (request.HttpMethod, request.Url?.AbsolutePath)
        {
            case ("GET", "/" or "/index.html"):
                ServePage(response);
                break;
            case ("POST", "/archive"):
                response.StatusCode = QueueArchive(request);
                break;
            default:
                response.StatusCode = 404;
                break;
        }
    }

    private void ServePage(HttpListenerResponse response)
    {
        if (!File.Exists(paths.Page))
        {
            response.StatusCode = 404;
            return;
        }
        var page = File.ReadAllBytes(paths.Page);
        response.StatusCode = 200;
        response.Headers["Cache-Control"] = "no-store";
        response.ContentType = "text/html; charset=utf-8";
        response.OutputStream.Write(page);
    }

    private int QueueArchive(HttpListenerRequest request)
    {
        if (request.Headers["Origin"] != Origin)
        {
            return 403;
        }
        var thread = ReadThread(request);
        var email = FeedJson.ReadIfExists<EmailsFeed>(paths.Emails)?.Emails.FirstOrDefault(e => thread != null && e.Thread == thread);
        if (email is null)
        {
            return 400;
        }
        try
        {
            archiveQueue.Add(new QueuedEmail(email.Thread, email.Subject));
            return 202;
        }
        catch (TimeoutException)
        {
            return 503;
        }
    }

    private static string? ReadThread(HttpListenerRequest request)
    {
        try
        {
            return JsonSerializer.Deserialize<ArchiveRequest>(request.InputStream, FeedJson.Options)?.Thread;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
