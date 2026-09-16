using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Admin;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/admin/vote-packages")]
[Authorize(Roles = "Admin")]
public class AdminVotePackagesController : ApiControllerBase
{
    private readonly IVotePackageAdminService _service;

    public AdminVotePackagesController(IVotePackageAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var packages = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<VotePackageAdminDto>>.Ok(packages));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVotePackageRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return FromResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVotePackageRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return FromResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var result = await _service.DeactivateAsync(id, ct);
        return FromResult(result);
    }
}
