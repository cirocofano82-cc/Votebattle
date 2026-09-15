namespace VoteBattle.Core.Entities;

/// <summary>
/// Records Stripe webhook events already processed, to guarantee idempotency:
/// a duplicated event never grants credits twice. The Stripe event id is the PK.
/// </summary>
public class ProcessedStripeEvent
{
    /// <summary>Stripe event id (e.g. "evt_..."). Primary key.</summary>
    public string StripeEventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
}
