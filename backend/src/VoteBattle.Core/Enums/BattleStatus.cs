namespace VoteBattle.Core.Enums;

/// <summary>
/// Publication / moderation status of a battle.
/// </summary>
public enum BattleStatus
{
    /// <summary>Created by a user, waiting for admin moderation. Not public.</summary>
    PendingModeration = 0,

    /// <summary>Approved and publicly visible; accepts votes.</summary>
    Active = 1,

    /// <summary>Rejected by an admin during moderation. Not public.</summary>
    Rejected = 2,

    /// <summary>Taken down by an admin after being public.</summary>
    Suspended = 3,

    /// <summary>Reached its end date; visible but no longer accepts votes.</summary>
    Ended = 4
}
