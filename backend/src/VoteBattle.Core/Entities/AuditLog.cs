using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Entities;

/// <summary>
/// A security / business audit event. Never stores sensitive information.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    public AuditEventType EventType { get; set; }

    /// <summary>Null when the event is system-generated (no acting user).</summary>
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string? IpAddress { get; set; }

    /// <summary>Non-sensitive structured details, stored as JSON.</summary>
    public string? Metadata { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
