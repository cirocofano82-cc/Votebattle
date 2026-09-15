using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.EventType).HasConversion<string>().HasMaxLength(48);
        builder.Property(a => a.IpAddress).HasMaxLength(64);

        // Non-sensitive structured details as PostgreSQL jsonb.
        builder.Property(a => a.Metadata).HasColumnType("jsonb");

        builder.HasIndex(a => a.EventType);
        builder.HasIndex(a => a.CreatedAt);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
