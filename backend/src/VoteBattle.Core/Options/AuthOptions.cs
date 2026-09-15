namespace VoteBattle.Core.Options;

/// <summary>
/// Authentication / account configuration.
/// </summary>
public class AuthOptions
{
    /// <summary>Base URL of the frontend, used to build email links.</summary>
    public string FrontendUrl { get; set; } = "http://localhost:3000";

    /// <summary>Lifetime of an email verification token, in hours.</summary>
    public int EmailVerificationTokenLifetimeHours { get; set; } = 24;

    /// <summary>Number of free credits granted once after email verification.</summary>
    public int RegistrationBonusCredits { get; set; } = 5;
}
