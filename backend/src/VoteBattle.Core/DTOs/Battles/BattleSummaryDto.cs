namespace VoteBattle.Core.DTOs.Battles;

/// <summary>
/// Compact battle representation for lists (homepage, discovery).
/// </summary>
public class BattleSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public decimal TotalAmountSpent { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<BattleParticipantDto> Participants { get; set; } = new();
}
