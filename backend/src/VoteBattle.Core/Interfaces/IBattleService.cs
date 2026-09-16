using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Battles;

namespace VoteBattle.Core.Interfaces;

public interface IBattleService
{
    Task<PagedResult<BattleSummaryDto>> GetBattlesAsync(BattleQueryParameters query, CancellationToken ct = default);

    Task<BattleDetailDto?> GetBattleBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Creates a battle in PENDING_MODERATION status. Returns the new slug.</summary>
    Task<Result<string>> CreateBattleAsync(Guid userId, CreateBattleRequest request, CancellationToken ct = default);

    /// <summary>Returns a battle for editing (owner or admin only), regardless of status.</summary>
    Task<Result<BattleDetailDto>> GetForEditAsync(Guid battleId, Guid userId, bool isAdmin, CancellationToken ct = default);

    /// <summary>Updates a battle's editable fields (owner or admin only). Slug is kept stable.</summary>
    Task<Result<string>> UpdateBattleAsync(Guid battleId, Guid userId, bool isAdmin, UpdateBattleRequest request, CancellationToken ct = default);
}
