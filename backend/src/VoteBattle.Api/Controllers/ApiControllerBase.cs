using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;

namespace VoteBattle.Api.Controllers;

/// <summary>
/// Base controller that maps a domain <see cref="Result"/> to a uniform HTTP response.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Best-effort client IP address for audit/anti-abuse.</summary>
    protected string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    protected IActionResult FromResult(Result result)
    {
        if (result.Succeeded)
            return Ok(ApiResponse.Ok(result.Message));

        var body = ApiResponse.Fail(result.Message, result.Errors);
        return StatusCode(MapStatusCode(result.ErrorType), body);
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.Succeeded)
            return Ok(ApiResponse<T>.Ok(result.Data!, result.Message));

        var body = ApiResponse<T>.Fail(result.Message, result.Errors);
        return StatusCode(MapStatusCode(result.ErrorType), body);
    }

    private static int MapStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest
    };
}
