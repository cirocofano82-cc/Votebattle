using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Payments;
using VoteBattle.Core.DTOs.Profile;

namespace VoteBattle.Core.Interfaces;

public interface IProfileService
{
    Task<ProfileStatsDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<PaymentDto>> GetPaymentsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
