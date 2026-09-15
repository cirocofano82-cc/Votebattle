namespace VoteBattle.Core.Entities;

/// <summary>
/// A comment left by a user on a battle.
/// </summary>
public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BattleId { get; set; }
    public Battle? Battle { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>Comment text (min 3, max 1000 characters — enforced by validation).</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Denormalized like count.</summary>
    public int LikeCount { get; set; }

    /// <summary>Soft delete: hidden by an admin but kept for audit.</summary>
    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}
