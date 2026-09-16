using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Admin;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin)
    {
        _admin = admin;
    }

    // Battles
    [HttpGet("battles")]
    public async Task<IActionResult> Battles(string? status, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _admin.GetBattlesAsync(status, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<AdminBattleDto>>.Ok(result));
    }

    [HttpPost("battles/{id:guid}/moderate")]
    public async Task<IActionResult> ModerateBattle(Guid id, [FromBody] ModerateBattleRequest request, CancellationToken ct)
    {
        var result = await _admin.ModerateBattleAsync(id, request, CurrentUserId ?? Guid.Empty, ct);
        return FromResult(result);
    }

    // Comments
    [HttpDelete("comments/{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken ct)
    {
        var result = await _admin.DeleteCommentAsync(id, ct);
        return FromResult(result);
    }

    // Reports
    [HttpGet("reports")]
    public async Task<IActionResult> Reports(string? status, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _admin.GetReportsAsync(status, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<AdminReportDto>>.Ok(result));
    }

    [HttpPost("reports/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportRequest request, CancellationToken ct)
    {
        var result = await _admin.ResolveReportAsync(id, request, ct);
        return FromResult(result);
    }

    // Users
    [HttpGet("users")]
    public async Task<IActionResult> Users(string? search, string? status, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _admin.GetUsersAsync(search, status, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.Ok(result));
    }

    [HttpPost("users/{id:guid}/status")]
    public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] SetUserStatusRequest request, CancellationToken ct)
    {
        var result = await _admin.SetUserStatusAsync(id, request, CurrentUserId ?? Guid.Empty, ct);
        return FromResult(result);
    }
}
