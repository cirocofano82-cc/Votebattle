using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Votes;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/votes")]
[Authorize]
public class VotesController : ApiControllerBase
{
    private readonly IVoteService _voteService;

    public VotesController(IVoteService voteService)
    {
        _voteService = voteService;
    }

    /// <summary>Casts one vote (spends one credit). Idempotency is not desired here:
    /// each successful call is a distinct vote.</summary>
    [HttpPost]
    public async Task<IActionResult> Vote([FromBody] CreateVoteRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _voteService.CastVoteAsync(userId.Value, request, ClientIp, ct);
        return FromResult(result);
    }
}
