using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Votes;

/// <summary>
/// Casts one or more votes (spends 1 credit per vote) for a contender in a battle.
/// A user may vote multiple times on the same battle, and may cast several votes
/// in a single request via <see cref="Quantity"/>.
/// </summary>
public class CreateVoteRequest
{
    /// <summary>Upper bound on how many votes can be cast in a single request.</summary>
    public const int MaxQuantity = 100;

    [Required]
    public Guid BattleId { get; set; }

    [Required]
    public Guid BattleParticipantId { get; set; }

    /// <summary>How many votes to cast at once. Each vote spends 1 Vote Credit.</summary>
    [Range(1, MaxQuantity, ErrorMessage = "You can cast between 1 and 100 votes at a time.")]
    public int Quantity { get; set; } = 1;
}
