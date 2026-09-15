using Microsoft.AspNetCore.Identity;
using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Entities;

/// <summary>
/// Application user. Extends ASP.NET Core Identity with domain fields.
/// Identity already provides Id, UserName, Email, PasswordHash, EmailConfirmed, etc.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Moderation / lifecycle status of the account.</summary>
    public UserStatus Status { get; set; } = UserStatus.EmailUnverified;

    /// <summary>
    /// Denormalized credit balance (a cache of the ledger). The source of truth is
    /// the sum of <see cref="VoteCreditTransaction"/> rows; this value is always
    /// written inside the same transaction as the matching ledger row.
    /// </summary>
    public int VoteCredits { get; set; }

    /// <summary>Optional avatar image URL.</summary>
    public string? AvatarUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    public ICollection<Battle> CreatedBattles { get; set; } = new List<Battle>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<VoteCreditTransaction> CreditTransactions { get; set; } = new List<VoteCreditTransaction>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();
}
