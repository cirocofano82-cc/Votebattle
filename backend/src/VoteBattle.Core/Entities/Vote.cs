namespace VoteBattle.Core.Entities;

/// <summary>
/// A single vote (one credit spent) cast by a user for a contender in a battle.
/// High-volume table with a bigint identity key.
///
/// NOTE: intentionally NO unique constraint on (UserId, BattleId) — a user may
/// vote multiple times on the same battle. Correctness is enforced by the atomic
/// credit-deduction transaction, not by a uniqueness constraint.
/// </summary>
public class Vote
{
    public long Id { get; set; }

    public Guid BattleId { get; set; }
    public Battle? Battle { get; set; }

    public Guid BattleParticipantId { get; set; }
    public BattleParticipant? BattleParticipant { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>The ledger row (VOTE_SPENT, -1) created together with this vote.</summary>
    public long CreditTransactionId { get; set; }
    public VoteCreditTransaction? CreditTransaction { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
