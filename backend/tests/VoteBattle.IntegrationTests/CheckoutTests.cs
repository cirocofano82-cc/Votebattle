using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoteBattle.Infrastructure.Data;
using VoteBattle.IntegrationTests.Infrastructure;
using Xunit;

namespace VoteBattle.IntegrationTests;

[Collection("integration")]
public class CheckoutTests
{
    private readonly VoteBattleWebAppFactory _factory;

    public CheckoutTests(VoteBattleWebAppFactory factory) => _factory = factory;

    /// <summary>
    /// When checkout cannot start (Stripe is not configured in the test environment),
    /// the request fails gracefully AND leaves no pending payment behind, so the
    /// user's purchase history stays clean.
    /// </summary>
    [Fact]
    public async Task Failed_checkout_leaves_no_payment_in_history()
    {
        var client = _factory.CreateClient();
        var user = await client.RegisterVerifyLoginAsync(_factory);
        var packageId = await GetActivePackageIdAsync();

        var resp = await client.PostAsJsonAsync("/api/payments/checkout", new { votePackageId = packageId });
        Assert.False(resp.IsSuccessStatusCode); // Stripe not configured -> cannot start

        var history = await client.GetAsync("/api/users/me/payments");
        history.EnsureSuccessStatusCode();
        var data = await history.ReadDataAsync();
        Assert.Equal(0, data.GetProperty("items").GetArrayLength());
    }

    private async Task<int> GetActivePackageIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.VotePackages.Where(p => p.IsActive).Select(p => p.Id).FirstAsync();
    }
}
