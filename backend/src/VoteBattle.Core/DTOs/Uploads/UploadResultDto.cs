namespace VoteBattle.Core.DTOs.Uploads;

/// <summary>Result of an image upload: the URL the client can use as an image source.</summary>
public class UploadResultDto
{
    public string Url { get; set; } = string.Empty;
}
