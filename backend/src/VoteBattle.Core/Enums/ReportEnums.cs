namespace VoteBattle.Core.Enums;

/// <summary>What kind of entity a report targets.</summary>
public enum ReportTargetType
{
    Comment = 0,
    Battle = 1,
    User = 2
}

/// <summary>Reason a user selected when filing a report.</summary>
public enum ReportReason
{
    Spam = 0,
    Harassment = 1,
    HateSpeech = 2,
    MisleadingContent = 3,
    Copyright = 4,
    Other = 5
}

/// <summary>Moderation status of a report.</summary>
public enum ReportStatus
{
    /// <summary>Filed, waiting for admin review.</summary>
    Pending = 0,

    /// <summary>Reviewed by an admin, no action taken yet.</summary>
    Reviewed = 1,

    /// <summary>Reviewed and acted upon (e.g. content removed, user banned).</summary>
    Actioned = 2,

    /// <summary>Reviewed and dismissed as not actionable.</summary>
    Dismissed = 3
}
