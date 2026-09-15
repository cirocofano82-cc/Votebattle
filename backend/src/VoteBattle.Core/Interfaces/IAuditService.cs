using VoteBattle.Core.Enums;

namespace VoteBattle.Core.Interfaces;

/// <summary>
/// Writes security / business audit events. Never log sensitive information
/// (passwords, tokens, card data).
/// </summary>
public interface IAuditService
{
    Task LogAsync(
        AuditEventType eventType,
        Guid? userId,
        string? ipAddress = null,
        object? metadata = null,
        CancellationToken ct = default);
}
