namespace VoteBattle.Core.Options;

/// <summary>
/// Configuration for locally-stored uploads (e.g. battle contender images).
/// </summary>
public class StorageOptions
{
    /// <summary>Folder where uploaded images are written (absolute, or relative to the app).</summary>
    public string UploadsPath { get; set; } = "uploads";

    /// <summary>Public base URL of the API, used to build absolute image URLs the browser can load.</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>Maximum accepted upload size, in bytes.</summary>
    public long MaxImageBytes { get; set; } = 2 * 1024 * 1024; // 2 MB
}
