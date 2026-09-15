using System.ComponentModel.DataAnnotations;

namespace VoteBattle.Core.DTOs.Auth;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
