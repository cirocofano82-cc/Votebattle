namespace VoteBattle.Core.Enums;

/// <summary>Moderation action an admin applies to a battle.</summary>
public enum BattleModerationAction
{
    Approve = 0, // -> Active
    Reject = 1,  // -> Rejected
    Suspend = 2  // -> Suspended
}

/// <summary>Moderation action an admin applies to a user.</summary>
public enum UserModerationAction
{
    Suspend = 0, // temporary
    Ban = 1,     // permanent
    Activate = 2 // lift suspension/ban
}
