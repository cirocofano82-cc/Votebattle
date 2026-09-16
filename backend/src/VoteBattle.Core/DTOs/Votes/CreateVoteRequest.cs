using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Votes;

/// <summary>
/// Casts a single vote (spends 1 credit) for a contender in a battle.
/// A user may vote multiple times on the same battle.
/// </summary>
public class CreateVoteRequest
{
    [Required]
    public Guid BattleId { get; set; }

    [Required]
    public Guid BattleParticipantId { get; set; }
}
