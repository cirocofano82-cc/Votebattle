namespace VoteBattle.Core.Enums;

/// <summary>
/// Lifecycle / moderation status of a user account.
/// </summary>
public enum UserStatus
{
    /// <summary>Registered but email not yet verified. Cannot vote, comment or buy.</summary>
    EmailUnverified = 0,

    /// <summary>Verified and in good standing.</summary>
    Active = 1,

    /// <summary>Temporarily blocked by an admin (see UserBan for details/expiry).</summary>
    Suspended = 2,

    /// <summary>Permanently blocked by an admin.</summary>
    Banned = 3,

    /// <summary>Flagged for review by anti-abuse tooling. Still readable by admins.</summary>
    Suspicious = 4
}
