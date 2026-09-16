using VoteBattle.Core.Common;

namespace VoteBattle.Core.Interfaces;

/// <summary>
/// Processes Stripe webhook deliveries. Verifies the signature and applies the effect
/// idempotently. This is the source of truth for payment confirmation.
/// </summary>
public interface IStripeWebhookService
{
    Task<Result> ProcessAsync(string requestJson, string? signatureHeader, CancellationToken ct = default);
}
