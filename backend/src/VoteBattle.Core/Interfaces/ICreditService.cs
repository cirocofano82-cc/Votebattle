using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Credits;

namespace VoteBattle.Core.Interfaces;

public interface ICreditService
{
    Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default);

    Task<PagedResult<CreditTransactionDto>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
