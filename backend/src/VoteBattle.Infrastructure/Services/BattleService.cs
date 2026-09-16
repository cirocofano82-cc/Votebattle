using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Battles;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Common;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class BattleService : IBattleService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _db;

    public BattleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<BattleSummaryDto>> GetBattlesAsync(BattleQueryParameters query, CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        // Only publicly visible battles are discoverable.
        var q = _db.Battles
            .AsNoTracking()
            .Where(b => b.Status == BattleStatus.Active);

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var categorySlug = query.Category.Trim().ToLowerInvariant();
            q = q.Where(b => b.Category!.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            q = q.Where(b => EF.Functions.ILike(b.Title, term));
        }

        var now = DateTimeOffset.UtcNow;
        q = (query.Sort?.Trim().ToLowerInvariant()) switch
        {
            "newest" => q.OrderByDescending(b => b.CreatedAt),
            "most-voted" => q.OrderByDescending(b => b.TotalVotes),
            "ending-soon" => q.Where(b => b.EndDate != null && b.EndDate > now).OrderBy(b => b.EndDate),
            // "trending" (default): most votes, then most viewed.
            _ => q.OrderByDescending(b => b.TotalVotes).ThenByDescending(b => b.ViewCount)
        };

        var totalCount = await q.CountAsync(ct);

        var battles = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(b => b.Category)
            .Include(b => b.Participants)
            .ToListAsync(ct);

        var items = battles.Select(MapSummary).ToList();
        return new PagedResult<BattleSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<BattleDetailDto?> GetBattleBySlugAsync(string slug, CancellationToken ct = default)
    {
        slug = slug.Trim().ToLowerInvariant();

        var battle = await _db.Battles
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Participants)
            .FirstOrDefaultAsync(b => b.Slug == slug, ct);

        if (battle is null)
            return null;

        // Only expose battles that are public (active or ended).
        if (battle.Status is not (BattleStatus.Active or BattleStatus.Ended))
            return null;

        // Best-effort view counter increment (does not block the response semantics).
        await _db.Battles
            .Where(b => b.Id == battle.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.ViewCount, b => b.ViewCount + 1), ct);

        return MapDetail(battle);
    }

    public async Task<Result<string>> CreateBattleAsync(Guid userId, CreateBattleRequest request, CancellationToken ct = default)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        if (!categoryExists)
            return Result<string>.Failure(ErrorType.Validation, "The selected category does not exist.");

        if (request.EndDate is not null && request.StartDate is not null && request.EndDate <= request.StartDate)
            return Result<string>.Failure(ErrorType.Validation, "End date must be after the start date.");

        var slug = await GenerateUniqueSlugAsync(request.Title, ct);
        var now = DateTimeOffset.UtcNow;

        var battle = new Battle
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            CreatedByUserId = userId,
            Status = BattleStatus.PendingModeration, // never public automatically
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = now,
            UpdatedAt = now,
            Participants = new List<BattleParticipant>
            {
                new()
                {
                    Name = request.CompetitorA.Name.Trim(),
                    Description = request.CompetitorA.Description?.Trim(),
                    ImageUrl = request.CompetitorA.ImageUrl?.Trim(),
                    Position = 1
                },
                new()
                {
                    Name = request.CompetitorB.Name.Trim(),
                    Description = request.CompetitorB.Description?.Trim(),
                    ImageUrl = request.CompetitorB.ImageUrl?.Trim(),
                    Position = 2
                }
            }
        };

        _db.Battles.Add(battle);
        await _db.SaveChangesAsync(ct);

        return Result<string>.Success(slug, "Battle submitted and is pending moderation.");
    }

    public async Task<Result<BattleDetailDto>> GetForEditAsync(Guid battleId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var battle = await _db.Battles
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.Participants)
            .FirstOrDefaultAsync(b => b.Id == battleId, ct);

        if (battle is null)
            return Result<BattleDetailDto>.Failure(ErrorType.NotFound, "Battle not found.");
        if (!isAdmin && battle.CreatedByUserId != userId)
            return Result<BattleDetailDto>.Failure(ErrorType.Forbidden, "You can only edit your own battles.");

        return Result<BattleDetailDto>.Success(MapDetail(battle));
    }

    public async Task<Result<string>> UpdateBattleAsync(
        Guid battleId, Guid userId, bool isAdmin, UpdateBattleRequest request, CancellationToken ct = default)
    {
        var battle = await _db.Battles
            .Include(b => b.Participants)
            .FirstOrDefaultAsync(b => b.Id == battleId, ct);

        if (battle is null)
            return Result<string>.Failure(ErrorType.NotFound, "Battle not found.");
        if (!isAdmin && battle.CreatedByUserId != userId)
            return Result<string>.Failure(ErrorType.Forbidden, "You can only edit your own battles.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        if (!categoryExists)
            return Result<string>.Failure(ErrorType.Validation, "The selected category does not exist.");

        battle.Title = request.Title.Trim();
        battle.Description = request.Description?.Trim();
        battle.CategoryId = request.CategoryId;
        battle.UpdatedAt = DateTimeOffset.UtcNow;

        // Update the two contenders by position (1 = A, 2 = B). Slug is kept stable
        // so existing links and SEO are not broken.
        var a = battle.Participants.FirstOrDefault(p => p.Position == 1);
        var b = battle.Participants.FirstOrDefault(p => p.Position == 2);
        if (a is not null)
        {
            a.Name = request.CompetitorA.Name.Trim();
            a.Description = request.CompetitorA.Description?.Trim();
            a.ImageUrl = request.CompetitorA.ImageUrl?.Trim();
        }
        if (b is not null)
        {
            b.Name = request.CompetitorB.Name.Trim();
            b.Description = request.CompetitorB.Description?.Trim();
            b.ImageUrl = request.CompetitorB.ImageUrl?.Trim();
        }

        await _db.SaveChangesAsync(ct);
        return Result<string>.Success(battle.Slug, "Battle updated.");
    }

    // --- Helpers ----------------------------------------------------------

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken ct)
    {
        var baseSlug = SlugGenerator.Generate(title);
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "battle";

        var slug = baseSlug;
        var suffix = 2;
        while (await _db.Battles.AnyAsync(b => b.Slug == slug, ct))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static BattleSummaryDto MapSummary(Battle b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Slug = b.Slug,
        CategoryName = b.Category?.Name ?? string.Empty,
        CategorySlug = b.Category?.Slug ?? string.Empty,
        TotalVotes = b.TotalVotes,
        TotalAmountSpent = b.TotalAmountSpent,
        EndDate = b.EndDate,
        CreatedAt = b.CreatedAt,
        Participants = b.Participants
            .OrderBy(p => p.Position)
            .Select(p => MapParticipant(p, b.TotalVotes))
            .ToList()
    };

    private static BattleDetailDto MapDetail(Battle b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Slug = b.Slug,
        Description = b.Description,
        Status = b.Status.ToString(),
        CreatedByUserId = b.CreatedByUserId,
        CategoryId = b.CategoryId,
        CategoryName = b.Category?.Name ?? string.Empty,
        CategorySlug = b.Category?.Slug ?? string.Empty,
        TotalVotes = b.TotalVotes,
        TotalAmountSpent = b.TotalAmountSpent,
        ViewCount = b.ViewCount,
        StartDate = b.StartDate,
        EndDate = b.EndDate,
        CreatedAt = b.CreatedAt,
        MetaDescription = b.MetaDescription,
        OgImageUrl = b.OgImageUrl,
        Participants = b.Participants
            .OrderBy(p => p.Position)
            .Select(p => MapParticipant(p, b.TotalVotes))
            .ToList()
    };

    private static BattleParticipantDto MapParticipant(BattleParticipant p, int totalVotes) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        Position = p.Position,
        VoteCount = p.VoteCount,
        Percentage = totalVotes > 0 ? Math.Round(p.VoteCount * 100.0 / totalVotes, 1) : 0
    };
}
