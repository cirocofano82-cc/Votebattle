using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.DTOs.Admin;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

        var paidPayments = _db.Payments.AsNoTracking().Where(p => p.Status == PaymentStatus.Paid);

        var totalRevenue = await paidPayments.SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        var revenueToday = await paidPayments.Where(p => p.CreatedAt >= todayStart)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        var paidCount = await paidPayments.CountAsync(ct);
        var payingUsers = await paidPayments.Select(p => p.UserId).Distinct().CountAsync(ct);

        var totalUsers = await _db.Users.CountAsync(ct);
        var totalBattles = await _db.Battles.CountAsync(ct);

        // Real votes come from the Votes table (seed demo counters are display-only).
        var totalVotes = await _db.Votes.CountAsync(ct);
        var votesToday = await _db.Votes.CountAsync(v => v.CreatedAt >= todayStart, ct);

        var freeCreditsDistributed = await _db.VoteCreditTransactions
            .Where(t => t.Type == CreditTransactionType.RegistrationBonus)
            .SumAsync(t => (int?)t.Amount, ct) ?? 0;
        var purchasedCredits = await _db.VoteCreditTransactions
            .Where(t => t.Type == CreditTransactionType.Purchase)
            .SumAsync(t => (int?)t.Amount, ct) ?? 0;
        var creditsSpent = await _db.VoteCreditTransactions
            .Where(t => t.Type == CreditTransactionType.VoteSpent)
            .SumAsync(t => (int?)-t.Amount, ct) ?? 0;

        var bonusUsers = await _db.VoteCreditTransactions
            .Where(t => t.Type == CreditTransactionType.RegistrationBonus)
            .Select(t => t.UserId).Distinct().CountAsync(ct);

        var topBattles = await _db.Battles.AsNoTracking()
            .OrderByDescending(b => b.TotalVotes)
            .Take(5)
            .Select(b => new TopBattleDto
            {
                Title = b.Title,
                Slug = b.Slug,
                TotalVotes = b.TotalVotes,
                TotalAmountSpent = b.TotalAmountSpent
            })
            .ToListAsync(ct);

        return new DashboardDto
        {
            TotalRevenue = totalRevenue,
            RevenueToday = revenueToday,
            TotalUsers = totalUsers,
            TotalBattles = totalBattles,
            TotalVotes = totalVotes,
            VotesToday = votesToday,
            PayingUsers = payingUsers,
            AverageOrderValue = paidCount > 0 ? Math.Round(totalRevenue / paidCount, 2) : 0m,
            AverageRevenuePerUser = totalUsers > 0 ? Math.Round(totalRevenue / totalUsers, 2) : 0m,
            ConversionRate = totalUsers > 0 ? Math.Round(payingUsers * 100.0 / totalUsers, 1) : 0,
            FreeToPaidConversionRate = bonusUsers > 0 ? Math.Round(payingUsers * 100.0 / bonusUsers, 1) : 0,
            FreeCreditsDistributed = freeCreditsDistributed,
            PurchasedCredits = purchasedCredits,
            CreditsSpent = creditsSpent,
            TopBattles = topBattles
        };
    }

    public async Task<SecurityOverviewDto> GetSecurityOverviewAsync(CancellationToken ct = default)
    {
        var weekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var dayAgo = DateTimeOffset.UtcNow.AddHours(-24);

        var recentUsers = await _db.Users.AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(10)
            .Select(u => new SuspiciousUserDto
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return new SecurityOverviewDto
        {
            SuspiciousUsers = await _db.Users.CountAsync(u => u.Status == UserStatus.Suspicious, ct),
            BannedUsers = await _db.Users.CountAsync(u => u.Status == UserStatus.Banned, ct),
            SuspendedUsers = await _db.Users.CountAsync(u => u.Status == UserStatus.Suspended, ct),
            RecentRegistrations = await _db.Users.CountAsync(u => u.CreatedAt >= weekAgo, ct),
            FailedLogins24h = await _db.AuditLogs.CountAsync(a => a.EventType == AuditEventType.LoginFailed && a.CreatedAt >= dayAgo, ct),
            RegistrationBonusesGranted = await _db.AuditLogs.CountAsync(a => a.EventType == AuditEventType.RegistrationBonusGranted, ct),
            RecentUsers = recentUsers
        };
    }
}
