using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class VotePackageConfiguration : IEntityTypeConfiguration<VotePackage>
{
    public void Configure(EntityTypeBuilder<VotePackage> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(64);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);

        builder.HasIndex(p => new { p.IsActive, p.DisplayOrder });
    }
}
