namespace VoteBattle.Core.DTOs.Payments;

public class VotePackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int Credits { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
}
