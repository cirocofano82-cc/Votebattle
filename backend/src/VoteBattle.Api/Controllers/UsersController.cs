using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Auth;
using VoteBattle.Core.DTOs.Credits;
using VoteBattle.Core.DTOs.Payments;
using VoteBattle.Core.DTOs.Profile;
using VoteBattle.Core.DTOs.Votes;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/users")]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICreditService _creditService;
    private readonly IVoteService _voteService;
    private readonly IProfileService _profileService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        ICreditService creditService,
        IVoteService voteService,
        IProfileService profileService)
    {
        _userManager = userManager;
        _creditService = creditService;
        _voteService = voteService;
        _profileService = profileService;
    }

    /// <summary>Returns the currently authenticated user (including credit balance).</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var roles = await _userManager.GetRolesAsync(user);
        var dto = new AuthUserDto
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Status = user.Status.ToString(),
            VoteCredits = user.VoteCredits,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList()
        };

        return Ok(ApiResponse<AuthUserDto>.Ok(dto));
    }

    /// <summary>Current Vote Credit balance.</summary>
    [HttpGet("me/credits")]
    public async Task<IActionResult> Credits(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var balance = await _creditService.GetBalanceAsync(userId.Value, ct);
        return Ok(ApiResponse<CreditBalanceDto>.Ok(new CreditBalanceDto { Balance = balance }));
    }

    /// <summary>Paginated credit history (ledger).</summary>
    [HttpGet("me/credits/history")]
    public async Task<IActionResult> CreditHistory(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var history = await _creditService.GetHistoryAsync(userId.Value, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<CreditTransactionDto>>.Ok(history));
    }

    /// <summary>Paginated vote history for the current user.</summary>
    [HttpGet("me/votes")]
    public async Task<IActionResult> Votes(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var votes = await _voteService.GetUserVotesAsync(userId.Value, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<UserVoteDto>>.Ok(votes));
    }

    /// <summary>Aggregated profile stats for the current user.</summary>
    [HttpGet("me/profile")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var profile = await _profileService.GetProfileAsync(userId.Value, ct);
        if (profile is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        return Ok(ApiResponse<ProfileStatsDto>.Ok(profile));
    }

    /// <summary>Paginated purchase history for the current user.</summary>
    [HttpGet("me/payments")]
    public async Task<IActionResult> Payments(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var payments = await _profileService.GetPaymentsAsync(userId.Value, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<PaymentDto>>.Ok(payments));
    }
}
