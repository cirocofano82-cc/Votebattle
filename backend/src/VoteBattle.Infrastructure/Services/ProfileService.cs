using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Payments;
using VoteBattle.Core.DTOs.Profile;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _db;

    public ProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProfileStatsDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return null;

        var totalVotes = await _db.Votes.CountAsync(v => v.UserId == userId, ct);
        var totalComments = await _db.Comments.CountAsync(c => c.UserId == userId && !c.IsDeleted, ct);
        var totalSpent = await _db.Payments
            .Where(p => p.UserId == userId && p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        return new ProfileStatsDto
        {
            Username = user.UserName ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            VoteCredits = user.VoteCredits,
            TotalVotes = totalVotes,
            TotalComments = totalComments,
            TotalSpent = totalSpent
        };
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var q = _db.Payments.AsNoTracking().Where(p => p.UserId == userId);
        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                PackageName = p.VotePackage!.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                VoteCreditsPurchased = p.VoteCreditsPurchased,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<PaymentDto>(items, page, pageSize, total);
    }
}
