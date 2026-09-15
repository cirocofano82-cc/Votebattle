using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class BattleParticipantConfiguration : IEntityTypeConfiguration<BattleParticipant>
{
    public void Configure(EntityTypeBuilder<BattleParticipant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.ImageUrl).HasMaxLength(512);

        builder.HasIndex(p => p.BattleId);

        // A battle exposes participants ordered by Position (1 = A, 2 = B).
        builder.HasIndex(p => new { p.BattleId, p.Position }).IsUnique();
    }
}
