using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Reports;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result> CreateReportAsync(Guid userId, CreateReportRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure(ErrorType.Unauthorized, "Not authenticated.");
        if (user.Status is UserStatus.Suspended or UserStatus.Banned)
            return Result.Failure(ErrorType.Forbidden, "Your account cannot submit reports.");

        var targetId = request.TargetId.Trim();

        // One report per user per target: treat a repeat as an idempotent success.
        var already = await _db.Reports.AnyAsync(r =>
            r.ReporterUserId == userId &&
            r.TargetType == request.TargetType &&
            r.TargetId == targetId, ct);
        if (already)
            return Result.Success("Thanks — you've already reported this. Our team will review it.");

        _db.Reports.Add(new Report
        {
            ReporterUserId = userId,
            TargetType = request.TargetType,
            TargetId = targetId,
            Reason = request.Reason,
            Details = request.Details?.Trim(),
            Status = ReportStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        return Result.Success("Thanks for reporting. Our team will review it.");
    }
}
