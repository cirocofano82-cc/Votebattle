using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Admin;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ApiControllerBase
{
    private readonly IAnalyticsService _analytics;

    public AdminAnalyticsController(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var data = await _analytics.GetDashboardAsync(ct);
        return Ok(ApiResponse<DashboardDto>.Ok(data));
    }

    [HttpGet("security")]
    public async Task<IActionResult> Security(CancellationToken ct)
    {
        var data = await _analytics.GetSecurityOverviewAsync(ct);
        return Ok(ApiResponse<SecurityOverviewDto>.Ok(data));
    }
}
