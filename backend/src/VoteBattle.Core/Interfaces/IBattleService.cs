using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Battles;

namespace VoteBattle.Core.Interfaces;

public interface IBattleService
{
    Task<PagedResult<BattleSummaryDto>> GetBattlesAsync(BattleQueryParameters query, CancellationToken ct = default);

    Task<BattleDetailDto?> GetBattleBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Creates a battle in PENDING_MODERATION status. Returns the new slug.</summary>
    Task<Result<string>> CreateBattleAsync(Guid userId, CreateBattleRequest request, CancellationToken ct = default);
}
