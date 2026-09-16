using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Reports;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/reports")]
[Authorize]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Generic report endpoint (comment, battle or user).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _reportService.CreateReportAsync(userId.Value, request, ct);
        return FromResult(result);
    }
}
