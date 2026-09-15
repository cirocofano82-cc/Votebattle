using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Auth;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Core.Options;
using VoteBattle.Infrastructure.Data;
using VoteBattle.Infrastructure.Security;

namespace VoteBattle.Infrastructure.Services;

/// <summary>
/// Authentication and account lifecycle. Handles registration (with anti-abuse checks),
/// single-use email verification, the one-time free-credit bonus, credential validation
/// for login, and password reset. Issuing the auth cookie is left to the API layer.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ICaptchaService _captchaService;
    private readonly IAuditService _audit;
    private readonly AuthOptions _authOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IEmailService emailService,
        ICaptchaService captchaService,
        IAuditService audit,
        IOptions<AuthOptions> authOptions,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
        _captchaService = captchaService;
        _audit = audit;
        _authOptions = authOptions.Value;
        _logger = logger;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken ct = default)
    {
        // 1. CAPTCHA (server-side; a no-op when disabled in development).
        if (!await _captchaService.VerifyAsync(request.CaptchaToken, ipAddress, ct))
            return Result.Failure(ErrorType.Validation, "CAPTCHA verification failed. Please try again.");

        var email = request.Email.Trim().ToLowerInvariant();

        // 2. Block disposable / temporary email domains.
        var domain = email.Contains('@') ? email[(email.IndexOf('@') + 1)..] : email;
        if (await _db.BlockedEmailDomains.AnyAsync(d => d.Domain == domain, ct))
            return Result.Failure(ErrorType.Validation, "Please use a permanent email address.");

        // 3. Create the user (unverified).
        var user = new ApplicationUser
        {
            UserName = request.Username.Trim(),
            Email = email,
            EmailConfirmed = false,
            Status = UserStatus.EmailUnverified,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return Result.Failure(ErrorType.Validation, "Registration failed.", errors);
        }

        // 4. Issue a single-use email verification token (store only its hash).
        await CreateAndSendVerificationTokenAsync(user, ct);

        await _audit.LogAsync(AuditEventType.UserRegistered, user.Id, ipAddress, ct: ct);

        return Result.Success("Registration successful. Please check your email to verify your account.");
    }

    public async Task<Result> VerifyEmailAsync(string token, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = TokenHasher.Hash(token);
        var now = DateTimeOffset.UtcNow;

        var tokenRow = await _db.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > now, ct);

        if (tokenRow?.User is null)
            return Result.Failure(ErrorType.Validation, "This verification link is invalid or has expired.");

        var user = tokenRow.User;

        // Already verified: consume this token idempotently and return success.
        if (user.EmailConfirmed)
        {
            tokenRow.UsedAt = now;
            await _db.SaveChangesAsync(ct);
            return Result.Success("Your email is already verified.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            tokenRow.UsedAt = now;
            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;
            user.UpdatedAt = now;

            var bonusGranted = false;
            var alreadyHasBonus = await _db.VoteCreditTransactions
                .AnyAsync(t => t.UserId == user.Id && t.Type == CreditTransactionType.RegistrationBonus, ct);

            if (!alreadyHasBonus)
            {
                var newBalance = user.VoteCredits + _authOptions.RegistrationBonusCredits;
                user.VoteCredits = newBalance;

                _db.VoteCreditTransactions.Add(new VoteCreditTransaction
                {
                    UserId = user.Id,
                    Type = CreditTransactionType.RegistrationBonus,
                    Amount = _authOptions.RegistrationBonusCredits,
                    BalanceAfter = newBalance,
                    ReferenceType = "Registration",
                    ReferenceId = user.Id.ToString(),
                    CreatedAt = now
                });
                bonusGranted = true;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            await _audit.LogAsync(AuditEventType.EmailVerified, user.Id, ipAddress, ct: ct);
            if (bonusGranted)
            {
                await _audit.LogAsync(AuditEventType.RegistrationBonusGranted, user.Id, ipAddress,
                    new { credits = _authOptions.RegistrationBonusCredits }, ct);
            }

            return Result.Success($"Email verified. You've received {_authOptions.RegistrationBonusCredits} free Vote Credits.");
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // A concurrent verification already granted the bonus; the email is verified.
            await tx.RollbackAsync(ct);
            _logger.LogInformation("Concurrent email verification detected for user {UserId}.", user.Id);
            return Result.Success("Email verified.");
        }
    }

    public async Task<Result> ResendVerificationAsync(string email, CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);

        // Only send when the account exists and is not yet verified.
        if (user is not null && !user.EmailConfirmed && user.Status == UserStatus.EmailUnverified)
        {
            await CreateAndSendVerificationTokenAsync(user, ct);
        }

        // Generic response to avoid leaking whether an email is registered.
        return Result.Success("If an unverified account exists for that email, a new verification link has been sent.");
    }

    public async Task<Result<ApplicationUser>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            await _audit.LogAsync(AuditEventType.LoginFailed, null, ipAddress, new { email }, ct);
            return Result<ApplicationUser>.Failure(ErrorType.Unauthorized, "Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
            return Result<ApplicationUser>.Failure(ErrorType.Unauthorized,
                "This account is temporarily locked due to failed sign-in attempts. Please try again later.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            await _audit.LogAsync(AuditEventType.LoginFailed, user.Id, ipAddress, ct: ct);
            return Result<ApplicationUser>.Failure(ErrorType.Unauthorized, "Invalid email or password.");
        }

        // Password correct: enforce account status.
        if (!user.EmailConfirmed || user.Status == UserStatus.EmailUnverified)
            return Result<ApplicationUser>.Failure(ErrorType.Forbidden, "Please verify your email before signing in.");

        if (user.Status == UserStatus.Suspended)
            return Result<ApplicationUser>.Failure(ErrorType.Forbidden, "Your account is suspended.");

        if (user.Status == UserStatus.Banned)
            return Result<ApplicationUser>.Failure(ErrorType.Forbidden, "Your account has been banned.");

        await _userManager.ResetAccessFailedCountAsync(user);
        return Result<ApplicationUser>.Success(user);
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is not null && user.Status is UserStatus.Active or UserStatus.Suspicious)
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = $"{_authOptions.FrontendUrl.TrimEnd('/')}/reset-password" +
                       $"?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(resetToken)}";

            await _emailService.SendEmailAsync(user.Email!, "Reset your VoteBattle password",
                BuildPasswordResetEmail(link), ct);
        }

        // Generic response to avoid leaking whether an email is registered.
        return Result.Success("If an account exists for that email, a password reset link has been sent.");
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure(ErrorType.Validation, "This password reset request is invalid.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result.Failure(ErrorType.Validation, "This password reset request is invalid or has expired.", errors);
        }

        return Result.Success("Your password has been reset. You can now sign in.");
    }

    // --- Helpers ----------------------------------------------------------

    private async Task CreateAndSendVerificationTokenAsync(ApplicationUser user, CancellationToken ct)
    {
        var rawToken = TokenHasher.GenerateToken();
        var tokenEntity = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(_authOptions.EmailVerificationTokenLifetimeHours),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.EmailVerificationTokens.Add(tokenEntity);
        await _db.SaveChangesAsync(ct);

        var link = $"{_authOptions.FrontendUrl.TrimEnd('/')}/verify-email?token={rawToken}";
        await _emailService.SendEmailAsync(user.Email!, "Verify your VoteBattle email",
            BuildVerificationEmail(link), ct);
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: "23505" };

    private static string BuildVerificationEmail(string link) => $"""
        <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto">
          <h2>Welcome to VoteBattle! 🎉</h2>
          <p>Confirm your email to activate your account and receive your <strong>5 free Vote Credits</strong>.</p>
          <p><a href="{link}" style="background:#4f46e5;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;display:inline-block">Verify my email</a></p>
          <p style="color:#666;font-size:13px">Or paste this link into your browser:<br>{link}</p>
          <p style="color:#999;font-size:12px">This link expires in 24 hours. If you didn't sign up, you can ignore this email.</p>
        </div>
        """;

    private static string BuildPasswordResetEmail(string link) => $"""
        <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto">
          <h2>Reset your password</h2>
          <p>We received a request to reset your VoteBattle password.</p>
          <p><a href="{link}" style="background:#4f46e5;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;display:inline-block">Reset my password</a></p>
          <p style="color:#666;font-size:13px">Or paste this link into your browser:<br>{link}</p>
          <p style="color:#999;font-size:12px">If you didn't request this, you can safely ignore this email.</p>
        </div>
        """;
}
