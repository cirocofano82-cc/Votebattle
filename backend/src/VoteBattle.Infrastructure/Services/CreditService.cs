using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Credits;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class CreditService : ICreditService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _db;

    public CreditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.VoteCredits)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<CreditTransactionDto>> GetHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var q = _db.VoteCreditTransactions.AsNoTracking().Where(t => t.UserId == userId);
        var totalCount = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows.Select(t => new CreditTransactionDto
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Description = Describe(t.Type),
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            CreatedAt = t.CreatedAt
        }).ToList();

        return new PagedResult<CreditTransactionDto>(items, page, pageSize, totalCount);
    }

    private static string Describe(CreditTransactionType type) => type switch
    {
        CreditTransactionType.RegistrationBonus => "Welcome bonus",
        CreditTransactionType.Purchase => "Purchased Vote Credits",
        CreditTransactionType.VoteSpent => "Voted in a battle",
        CreditTransactionType.AdminAdjustment => "Adjustment by admin",
        CreditTransactionType.Refund => "Refund",
        CreditTransactionType.Promotion => "Promotional credits",
        _ => type.ToString()
    };
}
