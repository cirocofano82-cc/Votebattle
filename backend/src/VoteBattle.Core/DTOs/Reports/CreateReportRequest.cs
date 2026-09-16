using System.ComponentModel.DataAnnotations;
using VoteBattle.Core.Enums;

namespace VoteBattle.Core.DTOs.Reports;

public class CreateReportRequest
{
    [Required]
    public ReportTargetType TargetType { get; set; }

    [Required]
    [StringLength(64)]
    public string TargetId { get; set; } = string.Empty;

    [Required]
    public ReportReason Reason { get; set; }

    [StringLength(2000)]
    public string? Details { get; set; }
}

/// <summary>Body for the comment-specific report shortcut (target is the route id).</summary>
public class ReportReasonRequest
{
    [Required]
    public ReportReason Reason { get; set; }

    [StringLength(2000)]
    public string? Details { get; set; }
}
