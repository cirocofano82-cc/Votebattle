using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.DTOs.Payments;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class VotePackageService : IVotePackageService
{
    private readonly AppDbContext _db;

    public VotePackageService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<VotePackageDto>> GetActivePackagesAsync(CancellationToken ct = default)
    {
        return await _db.VotePackages
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new VotePackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Currency = p.Currency,
                Credits = p.Credits,
                IsPopular = p.IsPopular,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(ct);
    }
}
