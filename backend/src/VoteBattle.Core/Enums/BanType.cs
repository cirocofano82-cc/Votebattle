namespace VoteBattle.Core.Enums;

/// <summary>Whether a moderation action is a temporary suspension or a permanent ban.</summary>
public enum BanType
{
    /// <summary>Temporary; may have an expiry date.</summary>
    Suspension = 0,

    /// <summary>Permanent.</summary>
    Ban = 1
}
