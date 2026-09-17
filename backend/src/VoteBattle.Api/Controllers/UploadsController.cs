using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Uploads;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/uploads")]
[Authorize(Roles = "Admin")]
public class UploadsController : ApiControllerBase
{
    private readonly IImageStorageService _storage;

    public UploadsController(IImageStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>Uploads an image and returns a URL to use as a contender/battle image.</summary>
    [HttpPost("image")]
    [RequestSizeLimit(3 * 1024 * 1024)] // hard cap above the service's own limit
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return FromResult(Result<UploadResultDto>.Failure(ErrorType.Validation, "No file was uploaded."));

        await using var stream = file.OpenReadStream();
        var result = await _storage.SaveImageAsync(stream, file.ContentType, file.Length, ct);
        return FromResult(result);
    }
}
