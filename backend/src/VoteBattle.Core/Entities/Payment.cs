using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Entities;

/// <summary>
/// A payment for a vote package via Stripe. The authoritative status transition to
/// Paid happens only from the verified Stripe webhook, never the frontend redirect.
/// </summary>
public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int VotePackageId { get; set; }
    public VotePackage? VotePackage { get; set; }

    public string? StripeCheckoutSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Credits granted by this purchase, snapshotted at checkout time so historical
    /// payments stay correct even if the package definition changes later.
    /// </summary>
    public int VoteCreditsPurchased { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // NOTE: the link to the credit ledger is polymorphic (VoteCreditTransaction.ReferenceType
    // = "Payment" and ReferenceId = this.Id). There is intentionally no navigation collection
    // here, to avoid a typed FK on a generic reference column.
}
