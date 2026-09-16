using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoteBattle.IntegrationTests.Infrastructure;
using VoteBattle.Infrastructure.Data;
using Xunit;

namespace VoteBattle.IntegrationTests;

[Collection("integration")]
public class VotingTests
{
    private readonly VoteBattleWebAppFactory _factory;

    public VotingTests(VoteBattleWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Voting_requires_authentication()
    {
        var client = _factory.CreateClient();
        var (battleId, participantId) = await client.GetFirstBattleAsync();

        var resp = await client.PostAsJsonAsync("/api/votes", new { battleId, battleParticipantId = participantId });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Vote_with_credits_decrements_balance()
    {
        var client = _factory.CreateClient();
        await client.RegisterVerifyLoginAsync(_factory);
        var (battleId, participantId) = await client.GetFirstBattleAsync();

        var resp = await client.PostAsJsonAsync("/api/votes", new { battleId, battleParticipantId = participantId });
        resp.EnsureSuccessStatusCode();

        var data = await resp.ReadDataAsync();
        Assert.Equal(4, data.GetProperty("newBalance").GetInt32()); // 5 -> 4
    }

    /// <summary>CRITICAL INVARIANT: no vote can be cast without a credit.</summary>
    [Fact]
    public async Task Vote_without_credits_is_rejected()
    {
        var client = _factory.CreateClient();
        var user = await client.RegisterVerifyLoginAsync(_factory);
        await SetCreditsAsync(user.Email, 0);

        var (battleId, participantId) = await client.GetFirstBattleAsync();
        var resp = await client.PostAsJsonAsync("/api/votes", new { battleId, battleParticipantId = participantId });

        Assert.Equal(HttpStatusCode.PaymentRequired, resp.StatusCode); // 402
    }

    /// <summary>
    /// CRITICAL INVARIANT: concurrent votes with a single credit must not overspend.
    /// Exactly one of many parallel votes succeeds; the balance never goes negative.
    /// </summary>
    [Fact]
    public async Task Concurrent_votes_with_one_credit_allow_only_one()
    {
        var client = _factory.CreateClient();
        var user = await client.RegisterVerifyLoginAsync(_factory);
        await SetCreditsAsync(user.Email, 1);

        var (battleId, participantId) = await client.GetFirstBattleAsync();

        var tasks = Enumerable.Range(0, 8).Select(_ =>
            client.PostAsJsonAsync("/api/votes", new { battleId, battleParticipantId = participantId }));
        var responses = await Task.WhenAll(tasks);

        var successes = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        Assert.Equal(1, successes);

        var balance = await GetCreditsAsync(user.Email);
        Assert.Equal(0, balance);
    }

    private async Task SetCreditsAsync(string email, int credits)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.Email == email);
        user.VoteCredits = credits;
        await db.SaveChangesAsync();
    }

    private async Task<int> GetCreditsAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Users.Where(u => u.Email == email).Select(u => u.VoteCredits).FirstAsync();
    }
}
