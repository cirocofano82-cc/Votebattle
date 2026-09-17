using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Uploads;

namespace VoteBattle.Core.Interfaces;

/// <summary>Stores uploaded images and returns a URL to serve them from.</summary>
public interface IImageStorageService
{
    Task<Result<UploadResultDto>> SaveImageAsync(
        Stream content, string? contentType, long length, CancellationToken ct = default);
}
