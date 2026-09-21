using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Infrastructure.Common;

namespace VoteBattle.Infrastructure.Data.Seed;

/// <summary>
/// Seeds baseline data: roles, an admin user, categories, vote packages, a starter
/// disposable-email blocklist and 10 demo battles. Every step is idempotent.
/// </summary>
public static class DataSeeder
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    public static async Task SeedAsync(
        AppDbContext db,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        string adminEmail,
        string adminPassword,
        ILogger logger,
        CancellationToken ct = default)
    {
        await SeedRolesAsync(roleManager);
        var admin = await SeedAdminUserAsync(userManager, adminEmail, adminPassword, logger);
        await SeedCategoriesAsync(db, ct);
        await SeedVotePackagesAsync(db, ct);
        await SeedBlockedEmailDomainsAsync(db, ct);
        await SeedBattlesAsync(db, admin.Id, ct);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in new[] { AdminRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role));
        }
    }

    private static async Task<ApplicationUser> SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        string adminEmail,
        string adminPassword,
        ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing is not null)
            return existing;

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed admin user: {errors}");
        }

        await userManager.AddToRoleAsync(admin, AdminRole);
        logger.LogInformation("Seeded admin user {Email}.", adminEmail);
        return admin;
    }

    private static async Task SeedCategoriesAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(ct))
            return;

        var names = new[]
        {
            "Technology", "Gaming", "Cars", "Movies", "Music",
            "Sports", "Food", "Fashion", "Brands", "Entertainment", "Other"
        };

        var order = 1;
        foreach (var name in names)
        {
            db.Categories.Add(new Category
            {
                Name = name,
                Slug = SlugGenerator.Generate(name),
                DisplayOrder = order++,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedVotePackagesAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.VotePackages.AnyAsync(ct))
            return;

        var now = DateTimeOffset.UtcNow;
        var packages = new[]
        {
            // Single Vote: visible but not highlighted (low display order, not popular).
            new VotePackage { Name = "Single Vote", Price = 1m, Currency = "USD", Credits = 1, IsActive = true, IsPopular = false, DisplayOrder = 1, CreatedAt = now, UpdatedAt = now },
            new VotePackage { Name = "Starter Pack", Price = 5m, Currency = "USD", Credits = 6, IsActive = true, IsPopular = false, DisplayOrder = 2, CreatedAt = now, UpdatedAt = now },
            new VotePackage { Name = "Popular Pack", Price = 10m, Currency = "USD", Credits = 13, IsActive = true, IsPopular = true, DisplayOrder = 3, CreatedAt = now, UpdatedAt = now },
            new VotePackage { Name = "Power Pack", Price = 25m, Currency = "USD", Credits = 35, IsActive = true, IsPopular = false, DisplayOrder = 4, CreatedAt = now, UpdatedAt = now },
            new VotePackage { Name = "Ultimate Pack", Price = 50m, Currency = "USD", Credits = 75, IsActive = true, IsPopular = false, DisplayOrder = 5, CreatedAt = now, UpdatedAt = now }
        };

        db.VotePackages.AddRange(packages);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedBlockedEmailDomainsAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.BlockedEmailDomains.AnyAsync(ct))
            return;

        // Starter set. In production, load a comprehensive open-source list.
        var domains = new[]
        {
            "mailinator.com", "guerrillamail.com", "10minutemail.com", "tempmail.com",
            "temp-mail.org", "throwawaymail.com", "yopmail.com", "getnada.com",
            "trashmail.com", "dispostable.com", "sharklasers.com", "maildrop.cc"
        };

        foreach (var d in domains)
            db.BlockedEmailDomains.Add(new BlockedEmailDomain { Domain = d });

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedBattlesAsync(AppDbContext db, Guid creatorUserId, CancellationToken ct)
    {
        var categories = await db.Categories.ToDictionaryAsync(c => c.Slug, c => c.Id, ct);

        var definitions = new[]
        {
            new BattleSeed("Samsung Galaxy Fold 8 vs iPhone Duo", "technology", "Which foldable-era flagship wins?", "Samsung Galaxy Fold 8", 7702, "iPhone Duo", 4736),
            new BattleSeed("PS5 vs Xbox Series X", "gaming", "The console showdown continues.", "PS5", 9120, "Xbox Series X", 6410),
            new BattleSeed("Tesla Model 3 vs BMW i4", "cars", "Electric sport sedans go head to head.", "Tesla Model 3", 5210, "BMW i4", 4890),
            new BattleSeed("Netflix vs Disney+", "entertainment", "Streaming giants battle for your screen time.", "Netflix", 8300, "Disney+", 5120),
            new BattleSeed("Spotify vs Apple Music", "music", "Where do you press play?", "Spotify", 9900, "Apple Music", 4300),
            new BattleSeed("Nike vs Adidas", "fashion", "The sneaker rivalry of a generation.", "Nike", 6600, "Adidas", 6100),
            new BattleSeed("MacBook Pro vs Dell XPS", "technology", "The ultimate pro laptop question.", "MacBook Pro", 7100, "Dell XPS", 3900),
            new BattleSeed("Google Pixel vs iPhone", "technology", "Stock Android vs iOS.", "Google Pixel", 4200, "iPhone", 7800),
            new BattleSeed("ChatGPT vs Gemini", "technology", "Which AI assistant do you trust?", "ChatGPT", 10200, "Gemini", 6700),
            new BattleSeed("Coca-Cola vs Pepsi", "food", "The taste test that never ends.", "Coca-Cola", 8800, "Pepsi", 5200)
        };

        var now = DateTimeOffset.UtcNow;
        foreach (var def in definitions)
        {
            var slug = SlugGenerator.Generate(def.Title);
            // IgnoreQueryFilters so a soft-deleted demo battle still counts as
            // existing — otherwise we'd re-insert its slug and hit the unique index.
            if (await db.Battles.IgnoreQueryFilters().AnyAsync(b => b.Slug == slug, ct))
                continue;

            if (!categories.TryGetValue(def.CategorySlug, out var categoryId))
                continue;

            var totalVotes = def.VotesA + def.VotesB;
            var battle = new Battle
            {
                Title = def.Title,
                Slug = slug,
                Description = def.Description,
                CategoryId = categoryId,
                CreatedByUserId = creatorUserId,
                Status = BattleStatus.Active,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(30),
                TotalVotes = totalVotes,
                TotalAmountSpent = totalVotes, // nominal $1 per vote
                ViewCount = totalVotes * 4,
                MetaDescription = def.Description,
                OgImageUrl = null,
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now,
                Participants = new List<BattleParticipant>
                {
                    // No external image: the UI shows a clean colored initial, which
                    // renders reliably even on networks that block image hosts.
                    new() { Name = def.NameA, Position = 1, VoteCount = def.VotesA, ImageUrl = null, Description = def.NameA },
                    new() { Name = def.NameB, Position = 2, VoteCount = def.VotesB, ImageUrl = null, Description = def.NameB }
                }
            };

            db.Battles.Add(battle);
        }

        await db.SaveChangesAsync(ct);

        // Clear any auto-generated demo image (old gray placeholder or brand logo)
        // so those contenders fall back to the colored initial. Admin-edited images
        // are left untouched, and once cleared this pass is a no-op.
        foreach (var def in definitions)
        {
            var slug = SlugGenerator.Generate(def.Title);
            var participants = await db.BattleParticipants
                .Where(p => p.Battle!.Slug == slug)
                .ToListAsync(ct);

            foreach (var p in participants)
            {
                if (p.ImageUrl is not null &&
                    (p.ImageUrl.Contains("placehold.co") || p.ImageUrl.Contains("logo.clearbit.com")))
                    p.ImageUrl = null;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private sealed record BattleSeed(
        string Title, string CategorySlug, string Description,
        string NameA, int VotesA, string NameB, int VotesB);
}
