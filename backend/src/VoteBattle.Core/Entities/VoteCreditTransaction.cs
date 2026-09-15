using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Entities;

/// <summary>
/// A single immutable entry in the vote-credit ledger. This is the source of truth
/// for a user's balance. The table is append-only: rows are never updated or deleted.
/// </summary>
public class VoteCreditTransaction
{
    public long Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public CreditTransactionType Type { get; set; }

    /// <summary>Signed amount: +5, -1, +13, ...</summary>
    public int Amount { get; set; }

    /// <summary>Resulting balance immediately after this transaction.</summary>
    public int BalanceAfter { get; set; }

    /// <summary>Polymorphic reference type, e.g. "Payment", "Vote", "Admin".</summary>
    public string? ReferenceType { get; set; }

    /// <summary>Identifier of the referenced entity (stored as string to stay generic).</summary>
    public string? ReferenceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
