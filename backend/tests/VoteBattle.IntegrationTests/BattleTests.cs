using System.Net;
using System.Net.Http.Json;
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
    public async Task Created_battle_is_pending_and_not_public()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);

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

        // Pending battles are not publicly retrievable.
        var get = await client.GetAsync($"/api/battles/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
