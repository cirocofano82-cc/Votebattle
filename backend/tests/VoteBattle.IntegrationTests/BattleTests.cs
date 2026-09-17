using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoteBattle.Infrastructure.Data;
using VoteBattle.IntegrationTests.Infrastructure;
using Xunit;

namespace VoteBattle.IntegrationTests;

[Collection("integration")]
public class BattleTests
{
    private readonly VoteBattleWebAppFactory _factory;

    public BattleTests(VoteBattleWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Battles_list_returns_seeded_battles()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/battles");
        resp.EnsureSuccessStatusCode();

        var data = await resp.ReadDataAsync();
        Assert.True(data.GetProperty("totalCount").GetInt32() >= 10);
    }

    [Fact]
    public async Task Battle_by_slug_returns_detail()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/battles/ps5-vs-xbox-series-x");
        resp.EnsureSuccessStatusCode();

        var data = await resp.ReadDataAsync();
        Assert.Equal("ps5-vs-xbox-series-x", data.GetProperty("slug").GetString());
        Assert.Equal(2, data.GetProperty("participants").GetArrayLength());
    }

    [Fact]
    public async Task Unknown_slug_returns_404()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/battles/this-does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Create_battle_requires_authentication()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/battles", new
        {
            title = "Anonymous vs Nobody",
            categoryId = 1,
            competitorA = new { name = "A" },
            competitorB = new { name = "B" }
        });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Regular_user_cannot_create_battle()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);

        var resp = await client.PostAsJsonAsync("/api/battles", new
        {
            title = "Player vs Player battle",
            categoryId = 1,
            competitorA = new { name = "A" },
            competitorB = new { name = "B" }
        });
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode); // 403: admin-only
    }

    [Fact]
    public async Task Admin_created_battle_is_live_immediately()
    {
        var client = _factory.CreateClient();
        await client.LoginAsAdminAsync();

        var unique = Guid.NewGuid().ToString("N")[..8];
        var title = $"Alpha{unique} vs Beta{unique}";
        var create = await client.PostAsJsonAsync("/api/battles", new
        {
            title,
            categoryId = 1,
            competitorA = new { name = "Alpha" },
            competitorB = new { name = "Beta" }
        });
        create.EnsureSuccessStatusCode();
        var slug = (await create.ReadDataAsync()).GetString();

        // Admin battles go live immediately, so they are publicly retrievable.
        var get = await client.GetAsync($"/api/battles/{slug}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var data = await get.ReadDataAsync();
        Assert.Equal("Active", data.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Admin_can_delete_a_suspended_battle_but_not_an_active_one()
    {
        var client = _factory.CreateClient();
        await client.LoginAsAdminAsync();

        var unique = Guid.NewGuid().ToString("N")[..8];
        var create = await client.PostAsJsonAsync("/api/battles", new
        {
            title = $"Del{unique} vs Test{unique}",
            categoryId = 1,
            competitorA = new { name = "Del" },
            competitorB = new { name = "Test" }
        });
        create.EnsureSuccessStatusCode();
        var slug = (await create.ReadDataAsync()).GetString();

        var detail = await (await client.GetAsync($"/api/battles/{slug}")).ReadDataAsync();
        var id = detail.GetProperty("id").GetString();

        // Active battles cannot be deleted.
        var delActive = await client.DeleteAsync($"/api/admin/battles/{id}");
        Assert.Equal(HttpStatusCode.BadRequest, delActive.StatusCode);

        // Suspend it, then deletion succeeds.
        var suspend = await client.PostAsJsonAsync($"/api/admin/battles/{id}/moderate", new { action = "Suspend" });
        suspend.EnsureSuccessStatusCode();

        var delSuspended = await client.DeleteAsync($"/api/admin/battles/{id}");
        delSuspended.EnsureSuccessStatusCode();

        // It is hidden from the site...
        var get = await client.GetAsync($"/api/battles/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        // ...but soft-deleted: the row is kept with IsDeleted = true.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.Battles.IgnoreQueryFilters()
            .FirstAsync(b => b.Slug == slug);
        Assert.True(row.IsDeleted);
    }

    [Fact]
    public async Task Admin_can_delete_a_rejected_battle()
    {
        var client = _factory.CreateClient();
        await client.LoginAsAdminAsync();

        var unique = Guid.NewGuid().ToString("N")[..8];
        var create = await client.PostAsJsonAsync("/api/battles", new
        {
            title = $"Rej{unique} vs Test{unique}",
            categoryId = 1,
            competitorA = new { name = "Rej" },
            competitorB = new { name = "Test" }
        });
        create.EnsureSuccessStatusCode();
        var slug = (await create.ReadDataAsync()).GetString();

        var detail = await (await client.GetAsync($"/api/battles/{slug}")).ReadDataAsync();
        var id = detail.GetProperty("id").GetString();

        var reject = await client.PostAsJsonAsync($"/api/admin/battles/{id}/moderate", new { action = "Reject" });
        reject.EnsureSuccessStatusCode();

        var del = await client.DeleteAsync($"/api/admin/battles/{id}");
        del.EnsureSuccessStatusCode();
    }
}
