namespace VoteBattle.Core.DTOs.Auth;

/// <summary>
/// Public representation of the authenticated user, returned by /users/me and after login.
/// </summary>
public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int VoteCredits { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}
