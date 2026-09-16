using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.IntegrationTests.Infrastructure;
using VoteBattle.Infrastructure.Data;
using Xunit;

namespace VoteBattle.IntegrationTests;

[Collection("integration")]
public class AuthBonusTests
{
    private readonly VoteBattleWebAppFactory _factory;

    public AuthBonusTests(VoteBattleWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_then_verify_grants_five_credits()
    {
        var client = _factory.CreateClient();
        var user = await client.RegisterVerifyLoginAsync(_factory);

        var me = await client.GetAsync("/api/users/me");
        me.EnsureSuccessStatusCode();
        var data = await me.ReadDataAsync();

        Assert.Equal(5, data.GetProperty("voteCredits").GetInt32());
        Assert.Equal("Active", data.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Register_with_duplicate_email_fails()
    {
        var client = _factory.CreateClient();
        var email = $"dup_{Guid.NewGuid():N}@example.com";

        var first = await client.PostAsJsonAsync("/api/auth/register",
            new { email, username = "user" + Guid.NewGuid().ToString("N")[..8], password = "Password123" });
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/api/auth/register",
            new { email, username = "user" + Guid.NewGuid().ToString("N")[..8], password = "Password123" });

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_before_verification_is_forbidden()
    {
        var client = _factory.CreateClient();
        var email = $"nv_{Guid.NewGuid():N}@example.com";
        var password = "Password123";

        var reg = await client.PostAsJsonAsync("/api/auth/register",
            new { email, username = "user" + Guid.NewGuid().ToString("N")[..8], password });
        reg.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.Forbidden, login.StatusCode);
    }

    [Fact]
    public async Task Verification_token_is_single_use()
    {
        var client = _factory.CreateClient();
        var email = $"su_{Guid.NewGuid():N}@example.com";

        await client.PostAsJsonAsync("/api/auth/register",
            new { email, username = "user" + Guid.NewGuid().ToString("N")[..8], password = "Password123" });
        var token = _factory.Email.ExtractVerifyToken(email)!;

        var first = await client.PostAsJsonAsync("/api/auth/verify-email", new { token });
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/api/auth/verify-email", new { token });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    /// <summary>
    /// CRITICAL INVARIANT: a user can never receive the registration bonus twice.
    /// Enforced by the partial unique index on the credit ledger.
    /// </summary>
    [Fact]
    public async Task Registration_bonus_cannot_be_granted_twice()
    {
        var client = _factory.CreateClient();
        var user = await client.RegisterVerifyLoginAsync(_factory);
        var userId = Guid.Parse(user.UserId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.VoteCreditTransactions.Add(new VoteCreditTransaction
        {
            UserId = userId,
            Type = CreditTransactionType.RegistrationBonus,
            Amount = 5,
            BalanceAfter = 10,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
