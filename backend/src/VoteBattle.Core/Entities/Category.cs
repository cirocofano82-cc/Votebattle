namespace VoteBattle.Core.Entities;

/// <summary>
/// Battle category (Technology, Gaming, Cars, ...).
/// </summary>
public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly slug used in filters, e.g. "technology".</summary>
    public string Slug { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Battle> Battles { get; set; } = new List<Battle>();
}
