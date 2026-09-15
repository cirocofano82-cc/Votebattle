using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class UserBanConfiguration : IEntityTypeConfiguration<UserBan>
{
    public void Configure(EntityTypeBuilder<UserBan> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(b => b.Reason).IsRequired().HasMaxLength(1000);

        builder.HasIndex(b => b.UserId);

        // Two FKs to ApplicationUser: both Restrict to avoid multiple cascade paths.
        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BannedByUser)
            .WithMany()
            .HasForeignKey(b => b.BannedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
