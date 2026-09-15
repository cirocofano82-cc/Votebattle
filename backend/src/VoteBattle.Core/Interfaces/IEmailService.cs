namespace VoteBattle.Core.Interfaces;

/// <summary>
/// Sends transactional emails. Implemented via SMTP (Mailhog in dev, a real provider
/// in production). Abstracted so the provider can change without touching business logic.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
