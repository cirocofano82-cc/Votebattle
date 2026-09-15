namespace VoteBattle.Core.Interfaces;

/// <summary>
/// Verifies a CAPTCHA challenge server-side. Never trust the client's result alone.
/// When CAPTCHA is disabled (development), the implementation returns true.
/// </summary>
public interface ICaptchaService
{
    Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken ct = default);
}
