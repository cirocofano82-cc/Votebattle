using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Auth;
using VoteBattle.Core.Entities;

namespace VoteBattle.Api.Controllers;

[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>Returns the currently authenticated user (including credit balance).</summary>
    [HttpGet("me")]
    [Authorize]
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
}
