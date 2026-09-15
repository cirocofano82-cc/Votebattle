using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.HasKey(v => v.Id);

        // High-volume table: bigint identity.
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.HasIndex(v => v.BattleId);
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => new { v.BattleId, v.BattleParticipantId });

        // IMPORTANT: intentionally NO unique index on (UserId, BattleId).

        builder.HasOne(v => v.Battle)
            .WithMany(b => b.Votes)
            .HasForeignKey(v => v.BattleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.BattleParticipant)
            .WithMany(p => p.Votes)
            .HasForeignKey(v => v.BattleParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.CreditTransaction)
            .WithMany()
            .HasForeignKey(v => v.CreditTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
