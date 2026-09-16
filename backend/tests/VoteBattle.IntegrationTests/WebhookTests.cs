using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.IntegrationTests.Infrastructure;
using VoteBattle.Infrastructure.Data;
using Xunit;

namespace VoteBattle.IntegrationTests;

[Collection("integration")]
public class WebhookTests
{
    private readonly VoteBattleWebAppFactory _factory;

    public WebhookTests(VoteBattleWebAppFactory factory) => _factory = factory;

    /// <summary>
    /// CRITICAL INVARIANT: a valid webhook grants the purchased credits exactly once;
    /// a duplicated delivery of the same event never grants them twice.
    /// </summary>
    [Fact]
    public async Task Valid_webhook_grants_credits_and_duplicate_is_ignored()
    {
        var client = _factory.CreateClient();
        var user = await client.RegisterVerifyLoginAsync(_factory); // starts at 5 credits
        var userId = Guid.Parse(user.UserId);

        Guid paymentId;
        int packageId, credits;
        var sessionId = "cs_test_" + Guid.NewGuid().ToString("N");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var package = await db.VotePackages.FirstAsync(p => p.IsActive && p.Credits >= 6);
            packageId = package.Id;
            credits = package.Credits;

            var payment = new Payment
            {
                UserId = userId,
                VotePackageId = packageId,
                Amount = package.Price,
                Currency = package.Currency,
                Status = PaymentStatus.Pending,
                VoteCreditsPurchased = credits,
                StripeCheckoutSessionId = sessionId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
            paymentId = payment.Id;
        }

        var eventId = "evt_test_" + Guid.NewGuid().ToString("N");
        var payload = BuildCheckoutCompletedPayload(eventId, sessionId, paymentId, userId, packageId);

        // First delivery grants credits.
        var first = await PostWebhookAsync(client, payload);
        Assert.True(first.IsSuccessStatusCode);
        Assert.Equal(5 + credits, await GetCreditsAsync(userId));

        // Duplicate delivery is ignored (no double grant).
        var duplicate = await PostWebhookAsync(client, payload);
        Assert.True(duplicate.IsSuccessStatusCode);
        Assert.Equal(5 + credits, await GetCreditsAsync(userId));
    }

    [Fact]
    public async Task Webhook_with_invalid_signature_is_rejected()
    {
        var client = _factory.CreateClient();
        var payload = BuildCheckoutCompletedPayload(
            "evt_" + Guid.NewGuid().ToString("N"), "cs_x", Guid.NewGuid(), Guid.NewGuid(), 1);

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/payments/webhook") { Content = content };
        req.Headers.Add("Stripe-Signature", $"t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()},v1=deadbeef");

        var resp = await client.SendAsync(req);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostWebhookAsync(HttpClient client, string payload)
    {
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/payments/webhook") { Content = content };
        req.Headers.Add("Stripe-Signature", Sign(payload, VoteBattleWebAppFactory.WebhookSecret));
        return await client.SendAsync(req);
    }

    private async Task<int> GetCreditsAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Users.Where(u => u.Id == userId).Select(u => u.VoteCredits).FirstAsync();
    }

    private static string BuildCheckoutCompletedPayload(
        string eventId, string sessionId, Guid paymentId, Guid userId, int packageId)
    {
        var payload = new
        {
            id = eventId,
            @object = "event",
            api_version = "2024-06-20",
            type = "checkout.session.completed",
            data = new
            {
                @object = new
                {
                    id = sessionId,
                    @object = "checkout.session",
                    payment_intent = "pi_test_" + Guid.NewGuid().ToString("N"),
                    metadata = new
                    {
                        paymentId = paymentId.ToString(),
                        userId = userId.ToString(),
                        votePackageId = packageId.ToString()
                    }
                }
            }
        };
        return System.Text.Json.JsonSerializer.Serialize(payload);
    }

    private static string Sign(string payload, string secret)
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{ts}.{payload}"));
        return $"t={ts},v1={Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
