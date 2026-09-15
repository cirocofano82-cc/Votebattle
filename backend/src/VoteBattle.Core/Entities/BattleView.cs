namespace VoteBattle.Core.Entities;

/// <summary>
/// A recorded view of a battle, used for analytics. High-volume table.
/// </summary>
public class BattleView
{
    public long Id { get; set; }

    public Guid BattleId { get; set; }
    public Battle? Battle { get; set; }

    /// <summary>Null for anonymous visitors.</summary>
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
