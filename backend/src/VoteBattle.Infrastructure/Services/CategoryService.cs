using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.DTOs.Battles;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                DisplayOrder = c.DisplayOrder
            })
            .ToListAsync(ct);
    }
}
