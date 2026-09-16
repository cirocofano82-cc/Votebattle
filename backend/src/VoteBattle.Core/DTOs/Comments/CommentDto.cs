namespace VoteBattle.Core.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid BattleId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
