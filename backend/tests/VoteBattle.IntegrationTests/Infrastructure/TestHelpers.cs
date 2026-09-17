using System.Net.Http.Json;
using System.Text.Json;

namespace VoteBattle.IntegrationTests.Infrastructure;

public record TestUser(string UserId, string Email, string Password);

public static class TestHelpers
{
    /// <summary>Registers a new user, verifies the email (granting the bonus) and logs in.
    /// The returned client carries the auth cookie.</summary>
    public static async Task<TestUser> RegisterVerifyLoginAsync(
        this HttpClient client, VoteBattleWebAppFactory factory, string password = "Password123")
    {
        var email = $"u_{Guid.NewGuid():N}@example.com";
        var username = "u" + Guid.NewGuid().ToString("N")[..10];

        var reg = await client.PostAsJsonAsync("/api/auth/register", new { email, username, password });
        reg.EnsureSuccessStatusCode();

        var token = factory.Email.ExtractVerifyToken(email)
            ?? throw new InvalidOperationException("Verification token was not captured.");
        var verify = await client.PostAsJsonAsync("/api/auth/verify-email", new { token });
        verify.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();

        var data = await ReadDataAsync(login);
        var userId = data.GetProperty("id").GetString()!;
        return new TestUser(userId, email, password);
    }

    /// <summary>Logs in as the seeded admin. The returned client carries the auth cookie.</summary>
    public static async Task LoginAsAdminAsync(this HttpClient client)
    {
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@votebattle.local", password = "Admin123!" });
        login.EnsureSuccessStatusCode();
    }

    /// <summary>Returns the "data" element of an ApiResponse body.</summary>
    public static async Task<JsonElement> ReadDataAsync(this HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("data").Clone();
    }

    /// <summary>Returns (battleId, participantId) of the first active battle.</summary>
    public static async Task<(string battleId, string participantId)> GetFirstBattleAsync(this HttpClient client)
    {
        var resp = await client.GetAsync("/api/battles?pageSize=1");
        resp.EnsureSuccessStatusCode();
        var data = await resp.ReadDataAsync();
        var battle = data.GetProperty("items")[0];
        var battleId = battle.GetProperty("id").GetString()!;
        var participantId = battle.GetProperty("participants")[0].GetProperty("id").GetString()!;
        return (battleId, participantId);
    }
}
