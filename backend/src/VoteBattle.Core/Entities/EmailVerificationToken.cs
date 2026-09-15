namespace VoteBattle.Core.Entities;

/// <summary>
/// A single-use email verification token. Only the hash of the token is stored;
/// the clear-text token is sent by email and never persisted.
/// </summary>
public class EmailVerificationToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>Hash of the token (e.g. SHA-256). The raw token lives only in the email.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>Set when the token is consumed. Null means not yet used (single-use).</summary>
    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
