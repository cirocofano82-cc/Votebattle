using VoteBattle.Core.DTOs.Battles;

namespace VoteBattle.Core.DTOs.Votes;

/// <summary>
/// State returned after a successful vote so the UI can refresh without a reload.
/// </summary>
public class VoteResultDto
{
    public int NewBalance { get; set; }
    public int BattleTotalVotes { get; set; }
    public decimal BattleTotalAmountSpent { get; set; }
    public List<BattleParticipantDto> Participants { get; set; } = new();
}
