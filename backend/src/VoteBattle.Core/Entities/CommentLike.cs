namespace VoteBattle.Core.Entities;

/// <summary>
/// A like on a comment. Unique per (CommentId, UserId): one like per user per comment.
/// </summary>
public class CommentLike
{
    public long Id { get; set; }

    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
