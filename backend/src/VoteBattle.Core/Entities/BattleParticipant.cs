namespace VoteBattle.Core.Entities;

/// <summary>
/// One of the two contenders in a battle. Each battle has exactly two of these.
/// </summary>
public class BattleParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BattleId { get; set; }
    public Battle? Battle { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>Display order within the battle: 1 = A (left), 2 = B (right).</summary>
    public short Position { get; set; }

    /// <summary>Denormalized number of votes for this contender.</summary>
    public int VoteCount { get; set; }

    // Navigation
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
