namespace VoteBattle.Core.DTOs.Credits;

/// <summary>
/// One entry of the user's credit history.
/// </summary>
public class CreditTransactionDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int BalanceAfter { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
