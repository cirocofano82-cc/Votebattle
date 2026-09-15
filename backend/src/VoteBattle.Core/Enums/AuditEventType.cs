namespace VoteBattle.Core.Enums;

/// <summary>
/// Types of security / business events written to the audit log.
/// Never store sensitive information (passwords, tokens, card data) in audit entries.
/// </summary>
public enum AuditEventType
{
    UserRegistered = 0,
    EmailVerified = 1,
    RegistrationBonusGranted = 2,
    LoginFailed = 3,
    AccountSuspended = 4,
    AccountBanned = 5,
    VoteCreated = 6,
    PaymentCompleted = 7,
    PaymentFailed = 8,
    PaymentRefunded = 9,
    StripeWebhookProcessed = 10,
    StripeWebhookDuplicate = 11
}
