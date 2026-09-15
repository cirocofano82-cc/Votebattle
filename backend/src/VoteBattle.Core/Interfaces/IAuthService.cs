using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Auth;
using VoteBattle.Core.Entities;

namespace VoteBattle.Core.Interfaces;

/// <summary>
/// Authentication and account lifecycle operations. Credential validation, email
/// verification and the free-credit bonus live here; issuing the auth cookie is done
/// by the API layer (an HTTP concern) using the user returned by <see cref="LoginAsync"/>.
/// </summary>
public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken ct = default);

    Task<Result> VerifyEmailAsync(string token, string? ipAddress, CancellationToken ct = default);

    Task<Result> ResendVerificationAsync(string email, CancellationToken ct = default);

    Task<Result<ApplicationUser>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);

    Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default);

    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
