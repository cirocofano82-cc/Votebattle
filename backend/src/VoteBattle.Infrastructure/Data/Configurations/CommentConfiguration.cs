using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content).IsRequired().HasMaxLength(1000);

        // Paginated "Newest first" listing per battle.
        builder.HasIndex(c => new { c.BattleId, c.CreatedAt });

        builder.HasOne(c => c.Battle)
            .WithMany(b => b.Comments)
            .HasForeignKey(c => c.BattleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Likes)
            .WithOne(l => l.Comment!)
            .HasForeignKey(l => l.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
