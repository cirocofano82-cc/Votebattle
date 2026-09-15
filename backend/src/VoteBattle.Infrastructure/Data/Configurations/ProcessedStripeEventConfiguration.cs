using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class ProcessedStripeEventConfiguration : IEntityTypeConfiguration<ProcessedStripeEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedStripeEvent> builder)
    {
        // The Stripe event id is the primary key => webhook idempotency at DB level.
        builder.HasKey(e => e.StripeEventId);

        builder.Property(e => e.StripeEventId).HasMaxLength(255);
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(128);
    }
}
