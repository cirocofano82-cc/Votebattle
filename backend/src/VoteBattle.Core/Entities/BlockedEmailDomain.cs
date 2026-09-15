namespace VoteBattle.Core.Entities;

/// <summary>
/// A disposable / temporary email domain blocked at registration time to reduce
/// mass multi-account abuse of the free-credit bonus.
/// </summary>
public class BlockedEmailDomain
{
    public int Id { get; set; }

    /// <summary>Lower-cased domain, e.g. "mailinator.com". Unique.</summary>
    public string Domain { get; set; } = string.Empty;
}
