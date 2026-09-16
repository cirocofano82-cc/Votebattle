using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Payments;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/vote-packages")]
public class VotePackagesController : ApiControllerBase
{
    private readonly IVotePackageService _packageService;

    public VotePackagesController(IVotePackageService packageService)
    {
        _packageService = packageService;
    }

    /// <summary>Lists active vote packages for the credits page (managed in the DB).</summary>
    [HttpGet]
    public async Task<IActionResult> GetPackages(CancellationToken ct)
    {
        var packages = await _packageService.GetActivePackagesAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<VotePackageDto>>.Ok(packages));
    }
}
