namespace VoteBattle.Core.DTOs.Battles;

/// <summary>
/// Full battle representation for the battle page.
/// </summary>
public class BattleDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;

    public int TotalVotes { get; set; }
    public decimal TotalAmountSpent { get; set; }
    public int ViewCount { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // SEO
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }

    public List<BattleParticipantDto> Participants { get; set; } = new();
}
