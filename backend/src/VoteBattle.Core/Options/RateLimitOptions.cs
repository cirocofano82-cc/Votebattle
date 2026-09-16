namespace VoteBattle.Core.Options;

/// <summary>
/// Per-minute request limits for sensitive endpoints (partitioned by user id, or IP
/// when anonymous). All values are configurable.
/// </summary>
public class RateLimitOptions
{
    /// <summary>register / login / forgot-password / resend-verification / reset.</summary>
    public int AuthPerMinute { get; set; } = 10;

    /// <summary>POST /api/votes. Generous: the credit check is the real guard.</summary>
    public int VotePerMinute { get; set; } = 120;

    /// <summary>Comment create / like / report.</summary>
    public int CommentPerMinute { get; set; } = 15;

    /// <summary>POST /api/payments/checkout.</summary>
    public int CheckoutPerMinute { get; set; } = 10;

    /// <summary>POST /api/payments/webhook. High, so legitimate Stripe bursts are not dropped.</summary>
    public int WebhookPerMinute { get; set; } = 300;
}
