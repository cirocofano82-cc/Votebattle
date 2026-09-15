namespace VoteBattle.Core.Enums;

/// <summary>
/// Type of a vote-credit ledger entry. The ledger is append-only:
/// every credit change is one immutable row of this type.
/// </summary>
public enum CreditTransactionType
{
    /// <summary>+5 free credits granted once, after email verification.</summary>
    RegistrationBonus = 0,

    /// <summary>Credits added after a confirmed Stripe payment.</summary>
    Purchase = 1,

    /// <summary>-1 credit spent to cast a vote.</summary>
    VoteSpent = 2,

    /// <summary>Manual correction performed by an admin.</summary>
    AdminAdjustment = 3,

    /// <summary>Credits removed because a payment was refunded.</summary>
    Refund = 4,

    /// <summary>Credits granted by a promotion / campaign.</summary>
    Promotion = 5
}
