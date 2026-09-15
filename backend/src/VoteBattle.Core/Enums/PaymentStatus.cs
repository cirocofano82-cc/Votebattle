namespace VoteBattle.Core.Enums;

/// <summary>
/// Status of a payment. The source of truth is the Stripe webhook,
/// never the frontend redirect.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Checkout session created, not yet paid.</summary>
    Pending = 0,

    /// <summary>Confirmed as paid via the Stripe webhook. Credits granted.</summary>
    Paid = 1,

    /// <summary>Payment attempt failed.</summary>
    Failed = 2,

    /// <summary>Refunded (fully or partially) after being paid.</summary>
    Refunded = 3,

    /// <summary>Cancelled before completion.</summary>
    Cancelled = 4
}
