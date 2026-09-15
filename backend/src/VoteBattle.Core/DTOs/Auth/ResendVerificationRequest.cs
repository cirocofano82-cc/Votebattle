using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Auth;

public class ResendVerificationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
