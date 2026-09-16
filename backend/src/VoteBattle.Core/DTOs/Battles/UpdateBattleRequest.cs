using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Battles;

public class UpdateBattleRequest
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    [Required]
    public CompetitorInput CompetitorA { get; set; } = new();

    [Required]
    public CompetitorInput CompetitorB { get; set; } = new();
}
