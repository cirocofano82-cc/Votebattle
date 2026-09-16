namespace VoteBattle.Core.DTOs.Profile;

/// <summary>Aggregated stats for the user's profile page.</summary>
public class ProfileStatsDto
{
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public int VoteCredits { get; set; }
    public int TotalVotes { get; set; }
    public int TotalComments { get; set; }
    public decimal TotalSpent { get; set; }
}
