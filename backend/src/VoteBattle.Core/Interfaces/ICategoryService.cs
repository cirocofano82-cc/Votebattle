using VoteBattle.Core.DTOs.Battles;

namespace VoteBattle.Core.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
}
