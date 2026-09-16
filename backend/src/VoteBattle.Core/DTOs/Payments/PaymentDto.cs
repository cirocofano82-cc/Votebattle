namespace VoteBattle.Core.DTOs.Payments;

/// <summary>One entry of the user's purchase history.</summary>
public class PaymentDto
{
    public Guid Id { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public int VoteCreditsPurchased { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
