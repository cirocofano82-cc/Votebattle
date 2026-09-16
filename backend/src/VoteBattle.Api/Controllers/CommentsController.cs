using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Comments;
using VoteBattle.Core.DTOs.Reports;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

public class CommentsController : ApiControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IReportService _reportService;

    public CommentsController(ICommentService commentService, IReportService reportService)
    {
        _commentService = commentService;
        _reportService = reportService;
    }

    /// <summary>Lists a battle's comments (newest first). Public; likedByMe set when signed in.</summary>
    [HttpGet("api/battles/{battleId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid battleId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _commentService.GetCommentsAsync(battleId, page, pageSize, CurrentUserId, ct);
        return Ok(ApiResponse<PagedResult<CommentDto>>.Ok(result));
    }

    [HttpPost("api/battles/{battleId:guid}/comments")]
    [Authorize]
    public async Task<IActionResult> CreateComment(Guid battleId, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _commentService.CreateCommentAsync(userId.Value, battleId, request, ClientIp, ct);
        return FromResult(result);
    }

    /// <summary>Toggles the current user's like on a comment.</summary>
    [HttpPost("api/comments/{id:guid}/like")]
    [Authorize]
    public async Task<IActionResult> ToggleLike(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _commentService.ToggleLikeAsync(userId.Value, id, ct);
        return FromResult(result);
    }

    /// <summary>Reports a comment.</summary>
    [HttpPost("api/comments/{id:guid}/report")]
    [Authorize]
    public async Task<IActionResult> ReportComment(Guid id, [FromBody] ReportReasonRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _reportService.CreateReportAsync(userId.Value, new CreateReportRequest
        {
            TargetType = ReportTargetType.Comment,
            TargetId = id.ToString(),
            Reason = request.Reason,
            Details = request.Details
        }, ct);
        return FromResult(result);
    }
}
