using Microsoft.AspNetCore.Identity;

namespace VoteBattle.Core.Entities;

/// <summary>
/// Application role (e.g. "Admin", "User"). Uses Guid keys to match ApplicationUser.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
