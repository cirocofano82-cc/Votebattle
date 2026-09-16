namespace VoteBattle.Core.Options;

/// <summary>
/// Comment anti-spam configuration (all values configurable).
/// </summary>
public class CommentOptions
{
    /// <summary>Minimum seconds between two comments by the same user.</summary>
    public int MinIntervalSeconds { get; set; } = 20;

    /// <summary>Window (minutes) in which an identical comment by the same user is rejected.</summary>
    public int DuplicateWindowMinutes { get; set; } = 10;
}
