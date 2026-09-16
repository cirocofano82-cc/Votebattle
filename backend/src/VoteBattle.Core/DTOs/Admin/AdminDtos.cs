using System.ComponentModel.DataAnnotations;
using VoteBattle.Core.Enums;

namespace VoteBattle.Core.DTOs.Admin;

public class AdminBattleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByUsername { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ModerateBattleRequest
{
    [Required]
    public BattleModerationAction Action { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int VoteCredits { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class SetUserStatusRequest
{
    [Required]
    public UserModerationAction Action { get; set; }

    [StringLength(1000)]
    public string? Reason { get; set; }

    /// <summary>Optional expiry for a suspension. Null means indefinite.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}

public class AdminReportDto
{
    public Guid Id { get; set; }
    public string ReporterUsername { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class ResolveReportRequest
{
    [Required]
    public ReportStatus Status { get; set; }
}
