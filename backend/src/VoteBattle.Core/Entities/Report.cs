using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Entities;

/// <summary>
/// A user report against a comment, battle or user.
/// </summary>
public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReporterUserId { get; set; }
    public ApplicationUser? ReporterUser { get; set; }

    public ReportTargetType TargetType { get; set; }

    /// <summary>Identifier of the reported entity (stored as string to stay generic).</summary>
    public string TargetId { get; set; } = string.Empty;

    public ReportReason Reason { get; set; }

    public string? Details { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
