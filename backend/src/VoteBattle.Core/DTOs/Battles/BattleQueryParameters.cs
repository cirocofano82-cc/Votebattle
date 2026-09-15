namespace VoteBattle.Core.DTOs.Battles;

/// <summary>
/// Query parameters for battle discovery (filters, search, sorting, pagination).
/// </summary>
public class BattleQueryParameters
{
    /// <summary>Optional category slug filter (e.g. "technology").</summary>
    public string? Category { get; set; }

    /// <summary>Free-text search over the battle title.</summary>
    public string? Search { get; set; }

    /// <summary>One of: trending, most-voted, newest, ending-soon. Defaults to trending.</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
