using Microsoft.Extensions.Options;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Uploads;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Core.Options;

namespace VoteBattle.Infrastructure.Storage;

/// <summary>
/// Saves uploaded images to a local folder and serves them via the API's
/// <c>/uploads</c> static path. The image type is detected from the file's magic
/// bytes (never trusting the client-supplied name or content type).
/// </summary>
public class LocalImageStorageService : IImageStorageService
{
    private readonly StorageOptions _options;

    public LocalImageStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Result<UploadResultDto>> SaveImageAsync(
        Stream content, string? contentType, long length, CancellationToken ct = default)
    {
        if (length <= 0)
            return Result<UploadResultDto>.Failure(ErrorType.Validation, "The file is empty.");
        if (length > _options.MaxImageBytes)
            return Result<UploadResultDto>.Failure(ErrorType.Validation,
                $"The image is too large (max {_options.MaxImageBytes / (1024 * 1024)} MB).");

        // Buffer the content (bounded by the size guard above) to inspect and write it.
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        if (bytes.Length == 0)
            return Result<UploadResultDto>.Failure(ErrorType.Validation, "The file is empty.");
        if (bytes.Length > _options.MaxImageBytes)
            return Result<UploadResultDto>.Failure(ErrorType.Validation,
                $"The image is too large (max {_options.MaxImageBytes / (1024 * 1024)} MB).");

        var ext = DetectExtension(bytes);
        if (ext is null)
            return Result<UploadResultDto>.Failure(ErrorType.Validation,
                "Unsupported image type. Use PNG, JPG, WEBP or GIF.");

        Directory.CreateDirectory(_options.UploadsPath);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(_options.UploadsPath, fileName);
        await File.WriteAllBytesAsync(fullPath, bytes, ct);

        var url = $"{_options.PublicBaseUrl.TrimEnd('/')}/uploads/{fileName}";
        return Result<UploadResultDto>.Success(new UploadResultDto { Url = url }, "Image uploaded.");
    }

    /// <summary>Detects a supported image type from its magic bytes, or null if unsupported.</summary>
    private static string? DetectExtension(byte[] b)
    {
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47)
            return ".png";
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ".jpg";
        if (b.Length >= 6 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x38)
            return ".gif";
        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
            b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50)
            return ".webp";
        return null;
    }
}
