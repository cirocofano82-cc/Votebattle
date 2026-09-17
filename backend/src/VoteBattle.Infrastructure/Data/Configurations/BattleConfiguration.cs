using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class BattleConfiguration : IEntityTypeConfiguration<Battle>
{
    public void Configure(EntityTypeBuilder<Battle> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Slug).IsRequired().HasMaxLength(220);
        builder.Property(b => b.Description).HasMaxLength(4000);
        builder.Property(b => b.MetaDescription).HasMaxLength(320);
        builder.Property(b => b.OgImageUrl).HasMaxLength(512);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(b => b.TotalAmountSpent).HasPrecision(18, 2);

        builder.HasIndex(b => b.Slug).IsUnique();
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.IsDeleted);

        // Soft-deleted battles are hidden from every query automatically.
        builder.HasQueryFilter(b => !b.IsDeleted);
        builder.HasIndex(b => b.CategoryId);
        builder.HasIndex(b => b.CreatedAt);
        builder.HasIndex(b => b.TotalVotes);
        builder.HasIndex(b => b.EndDate);

        builder.HasOne(b => b.Category)
            .WithMany(c => c.Battles)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Participants)
            .WithOne(p => p.Battle!)
            .HasForeignKey(p => p.BattleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
