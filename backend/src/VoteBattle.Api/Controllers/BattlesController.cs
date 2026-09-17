using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Battles;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/battles")]
public class BattlesController : ApiControllerBase
{
    private readonly IBattleService _battleService;
    private readonly ICategoryService _categoryService;

    public BattlesController(IBattleService battleService, ICategoryService categoryService)
    {
        _battleService = battleService;
        _categoryService = categoryService;
    }

    /// <summary>Discovery: filter by category, search, sort and paginate active battles.</summary>
    [HttpGet]
    public async Task<IActionResult> GetBattles([FromQuery] BattleQueryParameters query, CancellationToken ct)
    {
        var result = await _battleService.GetBattlesAsync(query, ct);
        return Ok(ApiResponse<PagedResult<BattleSummaryDto>>.Ok(result));
    }

    /// <summary>Lists active categories. (Literal route wins over the {slug} route.)</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _categoryService.GetCategoriesAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(categories));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var battle = await _battleService.GetBattleBySlugAsync(slug, ct);
        if (battle is null)
            return NotFound(ApiResponse.Fail("Battle not found."));

        return Ok(ApiResponse<BattleDetailDto>.Ok(battle));
    }

    /// <summary>Creates a battle. It starts in PENDING_MODERATION and is not public.
    /// Restricted to admins for now — user-submitted battles are disabled.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBattleRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _battleService.CreateBattleAsync(userId.Value, request, ct);
        return FromResult(result);
    }

    /// <summary>Returns a battle for editing (owner or admin), regardless of status.</summary>
    [HttpGet("{id:guid}/edit")]
    [Authorize]
    public async Task<IActionResult> GetForEdit(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _battleService.GetForEditAsync(id, userId.Value, IsAdmin, ct);
        return FromResult(result);
    }

    /// <summary>Updates a battle (owner or admin). Slug is kept stable.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBattleRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _battleService.UpdateBattleAsync(id, userId.Value, IsAdmin, request, ct);
        return FromResult(result);
    }
}
