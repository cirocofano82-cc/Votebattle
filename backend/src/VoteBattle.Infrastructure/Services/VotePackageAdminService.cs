using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Admin;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class VotePackageAdminService : IVotePackageAdminService
{
    private readonly AppDbContext _db;

    public VotePackageAdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<VotePackageAdminDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.VotePackages
            .AsNoTracking()
            .OrderBy(p => p.DisplayOrder)
            .Select(p => Map(p))
            .ToListAsync(ct);
    }

    public async Task<Result<VotePackageAdminDto>> CreateAsync(CreateVotePackageRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var pkg = new VotePackage
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Currency = request.Currency.ToUpperInvariant(),
            Credits = request.Credits,
            IsActive = request.IsActive,
            IsPopular = request.IsPopular,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.VotePackages.Add(pkg);
        await _db.SaveChangesAsync(ct);

        return Result<VotePackageAdminDto>.Success(Map(pkg), "Package created.");
    }

    public async Task<Result<VotePackageAdminDto>> UpdateAsync(int id, UpdateVotePackageRequest request, CancellationToken ct = default)
    {
        var pkg = await _db.VotePackages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pkg is null)
            return Result<VotePackageAdminDto>.Failure(ErrorType.NotFound, "Package not found.");

        // Editing a package never touches historical payments: each Payment snapshots
        // its own price and credits at purchase time.
        pkg.Name = request.Name.Trim();
        pkg.Price = request.Price;
        pkg.Currency = request.Currency.ToUpperInvariant();
        pkg.Credits = request.Credits;
        pkg.IsActive = request.IsActive;
        pkg.IsPopular = request.IsPopular;
        pkg.DisplayOrder = request.DisplayOrder;
        pkg.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Result<VotePackageAdminDto>.Success(Map(pkg), "Package updated.");
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var pkg = await _db.VotePackages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pkg is null)
            return Result.Failure(ErrorType.NotFound, "Package not found.");

        pkg.IsActive = false;
        pkg.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Result.Success("Package deactivated.");
    }

    private static VotePackageAdminDto Map(VotePackage p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Currency = p.Currency,
        Credits = p.Credits,
        IsActive = p.IsActive,
        IsPopular = p.IsPopular,
        DisplayOrder = p.DisplayOrder,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
