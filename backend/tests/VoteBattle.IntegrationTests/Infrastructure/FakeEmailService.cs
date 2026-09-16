using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.IntegrationTests.Infrastructure;

/// <summary>
/// Captures outgoing emails so tests can read the verification link/token instead of
/// sending real email.
/// </summary>
public class FakeEmailService : IEmailService
{
    private readonly ConcurrentDictionary<string, string> _lastBodyByEmail = new();

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        _lastBodyByEmail[toEmail.ToLowerInvariant()] = htmlBody;
        return Task.CompletedTask;
    }

    public string? ExtractVerifyToken(string email)
    {
        if (!_lastBodyByEmail.TryGetValue(email.ToLowerInvariant(), out var body))
            return null;
        var m = Regex.Match(body, @"verify-email\?token=([A-Za-z0-9_-]+)");
        return m.Success ? m.Groups[1].Value : null;
    }
}
