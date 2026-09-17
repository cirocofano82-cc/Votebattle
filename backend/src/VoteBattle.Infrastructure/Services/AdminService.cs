using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Admin;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class AdminService : IAdminService
{
    private const int MaxPageSize = 100;
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public AdminService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    // --- Battles ----------------------------------------------------------

    public async Task<PagedResult<AdminBattleDto>> GetBattlesAsync(string? status, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var q = _db.Battles.AsNoTracking().AsQueryable();
        if (Enum.TryParse<BattleStatus>(status, true, out var st))
            q = q.Where(b => b.Status == st);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new AdminBattleDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                CategoryName = b.Category!.Name,
                Status = b.Status.ToString(),
                CreatedByUsername = b.CreatedByUser!.UserName ?? "unknown",
                TotalVotes = b.TotalVotes,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<AdminBattleDto>(items, page, pageSize, total);
    }

    public async Task<Result> ModerateBattleAsync(Guid battleId, ModerateBattleRequest request, Guid adminId, CancellationToken ct = default)
    {
        var battle = await _db.Battles.FirstOrDefaultAsync(b => b.Id == battleId, ct);
        if (battle is null)
            return Result.Failure(ErrorType.NotFound, "Battle not found.");

        battle.Status = request.Action switch
        {
            BattleModerationAction.Approve => BattleStatus.Active,
            BattleModerationAction.Reject => BattleStatus.Rejected,
            BattleModerationAction.Suspend => BattleStatus.Suspended,
            _ => battle.Status
        };
        battle.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Result.Success($"Battle {request.Action.ToString().ToLowerInvariant()}d.");
    }

    public async Task<Result> DeleteBattleAsync(Guid battleId, Guid adminId, string? ipAddress, CancellationToken ct = default)
    {
        var battle = await _db.Battles.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == battleId, ct);
        if (battle is null)
            return Result.Failure(ErrorType.NotFound, "Battle not found.");

        // Guard: only suspended or ended battles can be removed.
        if (battle.Status is not (BattleStatus.Suspended or BattleStatus.Ended))
            return Result.Failure(ErrorType.Validation,
                "Only suspended or ended battles can be deleted.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // Remove rows that block deletion (Restrict FKs) first; the battle's
            // comments (and their likes) and views are removed by DB cascade.
            // The credit ledger keeps its rows — the spends really happened.
            await _db.Votes.Where(v => v.BattleId == battleId).ExecuteDeleteAsync(ct);
            await _db.BattleParticipants.Where(p => p.BattleId == battleId).ExecuteDeleteAsync(ct);
            await _db.Battles.Where(b => b.Id == battleId).ExecuteDeleteAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        await _audit.LogAsync(AuditEventType.BattleDeleted, adminId, ipAddress,
            new { battleId, title = battle.Title }, ct);

        return Result.Success("Battle deleted.");
    }

    // --- Comments ---------------------------------------------------------

    public async Task<Result> DeleteCommentAsync(Guid commentId, CancellationToken ct = default)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
        if (comment is null)
            return Result.Failure(ErrorType.NotFound, "Comment not found.");

        comment.IsDeleted = true; // soft delete: kept for audit
        comment.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Result.Success("Comment removed.");
    }

    // --- Reports ----------------------------------------------------------

    public async Task<PagedResult<AdminReportDto>> GetReportsAsync(string? status, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var q = _db.Reports.AsNoTracking().AsQueryable();
        if (Enum.TryParse<ReportStatus>(status, true, out var st))
            q = q.Where(r => r.Status == st);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new AdminReportDto
            {
                Id = r.Id,
                ReporterUsername = r.ReporterUser!.UserName ?? "unknown",
                TargetType = r.TargetType.ToString(),
                TargetId = r.TargetId,
                Reason = r.Reason.ToString(),
                Details = r.Details,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<AdminReportDto>(items, page, pageSize, total);
    }

    public async Task<Result> ResolveReportAsync(Guid reportId, ResolveReportRequest request, CancellationToken ct = default)
    {
        var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, ct);
        if (report is null)
            return Result.Failure(ErrorType.NotFound, "Report not found.");

        report.Status = request.Status;
        await _db.SaveChangesAsync(ct);

        return Result.Success("Report updated.");
    }

    // --- Users ------------------------------------------------------------

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(string? search, string? status, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var q = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            q = q.Where(u => EF.Functions.ILike(u.UserName!, term) || EF.Functions.ILike(u.Email!, term));
        }
        if (Enum.TryParse<UserStatus>(status, true, out var st))
            q = q.Where(u => u.Status == st);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Status = u.Status.ToString(),
                VoteCredits = u.VoteCredits,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<AdminUserDto>(items, page, pageSize, total);
    }

    public async Task<Result> SetUserStatusAsync(Guid userId, SetUserStatusRequest request, Guid adminId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure(ErrorType.NotFound, "User not found.");

        var now = DateTimeOffset.UtcNow;

        switch (request.Action)
        {
            case UserModerationAction.Suspend:
                user.Status = UserStatus.Suspended;
                _db.UserBans.Add(new UserBan
                {
                    UserId = userId, BannedByUserId = adminId, Type = BanType.Suspension,
                    Reason = request.Reason ?? "Suspended by admin", ExpiresAt = request.ExpiresAt, CreatedAt = now
                });
                await _db.SaveChangesAsync(ct);
                await _audit.LogAsync(AuditEventType.AccountSuspended, userId, null, new { by = adminId }, ct);
                return Result.Success("User suspended.");

            case UserModerationAction.Ban:
                user.Status = UserStatus.Banned;
                _db.UserBans.Add(new UserBan
                {
                    UserId = userId, BannedByUserId = adminId, Type = BanType.Ban,
                    Reason = request.Reason ?? "Banned by admin", ExpiresAt = null, CreatedAt = now
                });
                await _db.SaveChangesAsync(ct);
                await _audit.LogAsync(AuditEventType.AccountBanned, userId, null, new { by = adminId }, ct);
                return Result.Success("User banned.");

            case UserModerationAction.Activate:
                user.Status = UserStatus.Active;
                user.UpdatedAt = now;
                await _db.SaveChangesAsync(ct);
                return Result.Success("User reactivated.");

            default:
                return Result.Failure(ErrorType.Validation, "Unknown action.");
        }
    }
}
