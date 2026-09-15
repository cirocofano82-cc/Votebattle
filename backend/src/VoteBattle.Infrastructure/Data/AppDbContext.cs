using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Entities;

namespace VoteBattle.Infrastructure.Data;

/// <summary>
/// EF Core database context. Extends Identity (users, roles) and adds the domain tables.
/// Entity mapping (indexes, constraints, conversions) lives in the IEntityTypeConfiguration
/// classes under Data/Configurations, applied via ApplyConfigurationsFromAssembly.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Battle> Battles => Set<Battle>();
    public DbSet<BattleParticipant> BattleParticipants => Set<BattleParticipant>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<VotePackage> VotePackages => Set<VotePackage>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<VoteCreditTransaction> VoteCreditTransactions => Set<VoteCreditTransaction>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<UserBan> UserBans => Set<UserBan>();
    public DbSet<BattleView> BattleViews => Set<BattleView>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<ProcessedStripeEvent> ProcessedStripeEvents => Set<ProcessedStripeEvent>();
    public DbSet<BlockedEmailDomain> BlockedEmailDomains => Set<BlockedEmailDomain>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity mappings first.
        base.OnModelCreating(builder);

        // Then all domain configurations from this assembly.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
