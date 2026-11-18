using System.Net;
using System.Net.Sockets;
using System.Text;
using PersonalDashboard.Server;
using PersonalDashboard.Workspace;

namespace PersonalDashboard.Tests.Server;

public sealed class DashboardServerTests : IAsyncLifetime
{
    private readonly string root = Directory.CreateTempSubdirectory("dashboard-server-").FullName;
    private readonly int port = FreePort();
    private readonly HttpClient client = new();
    private DashboardServer? server;
    private WorkspacePaths Paths => new(root);
    private string Origin => $"http://127.0.0.1:{port}";

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(Paths.Output);
        File.WriteAllText(Paths.Page, "<html>page</html>");
        File.WriteAllText(Paths.Emails, """{ "refreshed": "x", "emails": [ { "from": "A", "subject": "Quote", "received": "d", "why": "w", "link": "", "thread": "t1" } ] }""");
        server = new DashboardServer(Paths, port);
        server.Start();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        server?.Dispose();
        client.Dispose();
        Directory.Delete(root, recursive: true);
        return Task.CompletedTask;
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private Task<HttpResponseMessage> PostArchive(string thread, string? origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Origin}/archive")
        {
            Content = new StringContent($$"""{"thread":"{{thread}}"}""", Encoding.UTF8, "application/json"),
        };
        if (origin is not null)
        {
            request.Headers.Add("Origin", origin);
        }
        return client.SendAsync(request);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/index.html")]
    public async Task Serves_the_page_without_caching(string path)
    {
        var response = await client.GetAsync($"{Origin}{path}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("<html>page</html>", await response.Content.ReadAsStringAsync());
        Assert.True(response.Headers.CacheControl!.NoStore);
    }

    [Fact]
    public async Task Missing_page_is_not_found()
    {
        File.Delete(Paths.Page);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Origin}/")).StatusCode);
    }

    [Fact]
    public async Task Archive_from_a_foreign_origin_is_forbidden()
    {
        Assert.Equal(HttpStatusCode.Forbidden, (await PostArchive("t1", "https://evil.example")).StatusCode);
    }

    [Fact]
    public async Task Archive_without_an_origin_is_forbidden()
    {
        Assert.Equal(HttpStatusCode.Forbidden, (await PostArchive("t1", null)).StatusCode);
    }

    [Fact]
    public async Task Archive_of_an_unknown_thread_is_a_bad_request()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await PostArchive("nope", Origin)).StatusCode);
    }

    [Fact]
    public async Task Archive_of_a_known_thread_is_queued_once()
    {
        var first = await PostArchive("t1", Origin);
        var second = await PostArchive("t1", Origin);

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, second.StatusCode);
        Assert.Equal([new QueuedEmail("t1", "Quote")], new ArchiveQueue(Paths.ArchiveQueue, Paths.ArchiveQueueLock).Read());
    }

    [Fact]
    public async Task Malformed_archive_body_is_a_bad_request()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Origin}/archive") { Content = new StringContent("not json") };
        request.Headers.Add("Origin", Origin);

        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Any_other_path_is_not_found()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Origin}/other")).StatusCode);
    }

    [Fact]
    public async Task Archive_is_unavailable_while_another_writer_holds_the_queue_lock()
    {
        File.WriteAllText(Paths.ArchiveQueueLock, "");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await PostArchive("t1", Origin)).StatusCode);
    }
}
