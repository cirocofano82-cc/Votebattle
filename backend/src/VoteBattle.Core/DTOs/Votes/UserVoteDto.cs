namespace VoteBattle.Core.DTOs.Votes;

public class UserVoteDto
{
    public long Id { get; set; }
    public Guid BattleId { get; set; }
    public string BattleTitle { get; set; } = string.Empty;
    public string BattleSlug { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
