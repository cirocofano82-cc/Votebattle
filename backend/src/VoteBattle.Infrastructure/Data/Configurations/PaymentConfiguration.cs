using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
        builder.Property(p => p.StripeCheckoutSessionId).HasMaxLength(255);
        builder.Property(p => p.StripePaymentIntentId).HasMaxLength(255);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(p => p.Status);

        // Unique when present, so a Stripe id maps to at most one payment.
        builder.HasIndex(p => p.StripeCheckoutSessionId)
            .IsUnique()
            .HasFilter("\"StripeCheckoutSessionId\" IS NOT NULL");

        builder.HasIndex(p => p.StripePaymentIntentId)
            .IsUnique()
            .HasFilter("\"StripePaymentIntentId\" IS NOT NULL");

        builder.HasOne(p => p.VotePackage)
            .WithMany(vp => vp.Payments)
            .HasForeignKey(p => p.VotePackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // No mapping for the credit ledger link: it is polymorphic via
        // VoteCreditTransaction.ReferenceType / ReferenceId (plain string columns).
    }
}
