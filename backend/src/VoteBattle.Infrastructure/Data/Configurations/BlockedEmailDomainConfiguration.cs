using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class BlockedEmailDomainConfiguration : IEntityTypeConfiguration<BlockedEmailDomain>
{
    public void Configure(EntityTypeBuilder<BlockedEmailDomain> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Domain).IsRequired().HasMaxLength(255);

        builder.HasIndex(d => d.Domain).IsUnique();
    }
}
