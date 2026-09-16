using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Admin;

namespace VoteBattle.Core.Interfaces;

public interface IAdminService
{
    Task<PagedResult<AdminBattleDto>> GetBattlesAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task<Result> ModerateBattleAsync(Guid battleId, ModerateBattleRequest request, Guid adminId, CancellationToken ct = default);

    Task<Result> DeleteCommentAsync(Guid commentId, CancellationToken ct = default);

    Task<PagedResult<AdminReportDto>> GetReportsAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task<Result> ResolveReportAsync(Guid reportId, ResolveReportRequest request, CancellationToken ct = default);

    Task<PagedResult<AdminUserDto>> GetUsersAsync(string? search, string? status, int page, int pageSize, CancellationToken ct = default);
    Task<Result> SetUserStatusAsync(Guid userId, SetUserStatusRequest request, Guid adminId, CancellationToken ct = default);
}
