using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class VoteCreditTransactionConfiguration : IEntityTypeConfiguration<VoteCreditTransaction>
{
    public void Configure(EntityTypeBuilder<VoteCreditTransaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(t => t.ReferenceType).HasMaxLength(64);
        builder.Property(t => t.ReferenceId).HasMaxLength(64);

        // Credit history for a user, ordered by time.
        builder.HasIndex(t => new { t.UserId, t.CreatedAt });

        // A user can receive the REGISTRATION_BONUS at most once.
        // Type is stored as string, so the partial filter matches the enum name.
        builder.HasIndex(t => t.UserId)
            .IsUnique()
            .HasDatabaseName("IX_VoteCreditTransactions_UserId_RegistrationBonus_Unique")
            .HasFilter("\"Type\" = 'RegistrationBonus'");
    }
}
