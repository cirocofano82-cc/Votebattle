namespace VoteBattle.Core.DTOs.Payments;

/// <summary>
/// The Stripe Checkout Session to which the frontend must redirect the user.
/// </summary>
public class CheckoutSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
