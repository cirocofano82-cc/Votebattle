using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Entities;

/// <summary>
/// A suspension or ban applied to a user by an admin.
/// </summary>
public class UserBan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public Guid BannedByUserId { get; set; }
    public ApplicationUser? BannedByUser { get; set; }

    public BanType Type { get; set; }

    public string Reason { get; set; } = string.Empty;

    /// <summary>Null means permanent (typically for a Ban).</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
