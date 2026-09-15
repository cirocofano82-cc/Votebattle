namespace VoteBattle.Core.Entities;

/// <summary>
/// A purchasable package of vote credits. Managed centrally (DB + admin),
/// never hardcoded in the frontend.
/// </summary>
public class VotePackage
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public int Credits { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>When true, shown with a "MOST POPULAR" badge.</summary>
    public bool IsPopular { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
