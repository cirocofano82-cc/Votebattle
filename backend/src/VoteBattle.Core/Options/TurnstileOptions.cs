namespace VoteBattle.Core.Options;

/// <summary>
/// Cloudflare Turnstile configuration. In development CAPTCHA can be disabled;
/// in production it must be enabled.
/// </summary>
public class TurnstileOptions
{
    public bool Enabled { get; set; }
    public string SiteKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Cloudflare server-side verification endpoint.</summary>
    public string VerifyUrl { get; set; } = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
}
