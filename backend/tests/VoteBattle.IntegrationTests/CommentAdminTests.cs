using System.Net;
using System.Net.Http.Json;
using VoteBattle.IntegrationTests.Infrastructure;
using Xunit;

namespace VoteBattle.IntegrationTests;

[Collection("integration")]
public class CommentAdminTests
{
    private readonly VoteBattleWebAppFactory _factory;

    public CommentAdminTests(VoteBattleWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Comment_create_then_immediate_repeat_is_rate_limited()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);
        var (battleId, _) = await client.GetFirstBattleAsync();

        var first = await client.PostAsJsonAsync($"/api/battles/{battleId}/comments",
            new { content = "This is my first take on the matter." });
        first.EnsureSuccessStatusCode();

        // A second comment immediately after hits the per-user anti-spam interval.
        var second = await client.PostAsJsonAsync($"/api/battles/{battleId}/comments",
            new { content = "A different second comment posted right away." });
        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
    }

    [Fact]
    public async Task Too_short_comment_is_rejected()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);
        var (battleId, _) = await client.GetFirstBattleAsync();

        var resp = await client.PostAsJsonAsync($"/api/battles/{battleId}/comments", new { content = "hi" });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Admin_endpoints_require_authentication()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Admin_endpoints_forbidden_for_non_admin()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);

        var resp = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Admin_can_delete_a_comment()
    {
        // A user posts a comment.
        var userClient = _factory.CreateClient();
        await userClient.RegisterVerifyLoginAsync(_factory);
        var (battleId, _) = await userClient.GetFirstBattleAsync();

        var post = await userClient.PostAsJsonAsync($"/api/battles/{battleId}/comments",
            new { content = "A comment an admin will remove." });
        post.EnsureSuccessStatusCode();
        var commentId = (await post.ReadDataAsync()).GetProperty("id").GetString();

        // An admin deletes it.
        var adminClient = _factory.CreateClient();
        await adminClient.LoginAsAdminAsync();
        var del = await adminClient.DeleteAsync($"/api/admin/comments/{commentId}");
        del.EnsureSuccessStatusCode();

        // It no longer appears in the battle's comments.
        var list = await (await userClient.GetAsync($"/api/battles/{battleId}/comments")).ReadDataAsync();
        foreach (var c in list.GetProperty("items").EnumerateArray())
            Assert.NotEqual(commentId, c.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Non_admin_cannot_delete_a_comment()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);

        var resp = await client.DeleteAsync($"/api/admin/comments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Admin_can_access_admin_endpoints()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@votebattle.local", password = "Admin123!" });
        login.EnsureSuccessStatusCode();

        var resp = await client.GetAsync("/api/admin/users");
        resp.EnsureSuccessStatusCode();
    }
}
