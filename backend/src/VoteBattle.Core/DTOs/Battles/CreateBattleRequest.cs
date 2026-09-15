using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Battles;

public class CreateBattleRequest
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    [Required]
    public CompetitorInput CompetitorA { get; set; } = new();

    [Required]
    public CompetitorInput CompetitorB { get; set; } = new();
}

public class CompetitorInput
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(512)]
    [Url]
    public string? ImageUrl { get; set; }
}
