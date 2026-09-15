using System.Text.Json;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Services;

/// <summary>
/// Persists audit events to the database. Metadata is serialized to JSON; callers must
/// never pass sensitive information.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        AuditEventType eventType,
        Guid? userId,
        string? ipAddress = null,
        object? metadata = null,
        CancellationToken ct = default)
    {
        var entry = new AuditLog
        {
            EventType = eventType,
            UserId = userId,
            IpAddress = ipAddress,
            Metadata = metadata is null ? null : JsonSerializer.Serialize(metadata),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync(ct);
    }
}
