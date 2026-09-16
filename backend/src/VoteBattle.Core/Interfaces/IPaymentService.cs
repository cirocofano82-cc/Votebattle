using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Payments;

namespace VoteBattle.Core.Interfaces;

/// <summary>
/// Payment provider abstraction. The rest of the application depends on this, not on
/// Stripe directly, so another provider can be added later.
/// </summary>
public interface IPaymentService
{
    Task<Result<CheckoutSessionDto>> CreateCheckoutSessionAsync(Guid userId, int votePackageId, CancellationToken ct = default);
}
