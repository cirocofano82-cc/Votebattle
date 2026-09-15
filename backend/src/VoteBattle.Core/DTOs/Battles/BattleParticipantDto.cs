namespace VoteBattle.Core.DTOs.Battles;

public class BattleParticipantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public short Position { get; set; }
    public int VoteCount { get; set; }

    /// <summary>Share of total battle votes (0–100), computed server-side.</summary>
    public double Percentage { get; set; }
}
