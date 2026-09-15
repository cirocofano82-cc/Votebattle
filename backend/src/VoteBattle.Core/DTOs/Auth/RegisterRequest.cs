using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$",
        ErrorMessage = "Username may only contain letters, numbers and _ . - characters.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Cloudflare Turnstile token. Required only when CAPTCHA is enabled.</summary>
    public string? CaptchaToken { get; set; }
}
