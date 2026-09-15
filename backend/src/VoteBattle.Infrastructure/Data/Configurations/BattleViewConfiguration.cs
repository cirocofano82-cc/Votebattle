using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class BattleViewConfiguration : IEntityTypeConfiguration<BattleView>
{
    public void Configure(EntityTypeBuilder<BattleView> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.HasIndex(v => v.BattleId);

        builder.HasOne(v => v.Battle)
            .WithMany(b => b.Views)
            .HasForeignKey(v => v.BattleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
