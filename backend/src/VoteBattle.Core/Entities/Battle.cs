using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Entities;

/// <summary>
/// A head-to-head battle between exactly two contenders (BattleParticipants).
/// </summary>
public class Battle
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    /// <summary>Unique SEO-friendly slug, e.g. "samsung-galaxy-fold-8-vs-iphone-duo".</summary>
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    public BattleStatus Status { get; set; } = BattleStatus.PendingModeration;

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    // --- Denormalized counters (caches; kept in sync inside vote transactions) ---

    /// <summary>Total votes across both participants.</summary>
    public int TotalVotes { get; set; }

    /// <summary>Nominal amount spent on this battle (TotalVotes * $1). See design decision.</summary>
    public decimal TotalAmountSpent { get; set; }

    /// <summary>Number of views recorded for this battle.</summary>
    public int ViewCount { get; set; }

    // --- SEO ---
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }

    /// <summary>Soft-delete flag. Deleted battles are hidden from every query
    /// via a global filter, but their rows (and vote history) are kept.</summary>
    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<BattleParticipant> Participants { get; set; } = new List<BattleParticipant>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<BattleView> Views { get; set; } = new List<BattleView>();
}
