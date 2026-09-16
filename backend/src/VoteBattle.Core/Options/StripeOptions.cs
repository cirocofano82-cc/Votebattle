namespace VoteBattle.Core.Options;

/// <summary>
/// Stripe configuration. Use test keys in development and live keys only in production.
/// </summary>
public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
