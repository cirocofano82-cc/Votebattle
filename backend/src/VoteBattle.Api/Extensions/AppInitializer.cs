using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VoteBattle.Core.Entities;
using VoteBattle.Infrastructure.Data;
using VoteBattle.Infrastructure.Data.Seed;

namespace VoteBattle.Api.Extensions;

/// <summary>
/// Applies pending EF Core migrations and seeds baseline data at startup.
/// </summary>
public static class AppInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AppInitializer");
        var config = services.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync();

        var adminEmail = config["Seed:AdminEmail"]
            ?? Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL")
            ?? "admin@votebattle.local";
        var adminPassword = config["Seed:AdminPassword"]
            ?? Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD")
            ?? "Admin123!";

        await DataSeeder.SeedAsync(db, roleManager, userManager, adminEmail, adminPassword, logger);
        logger.LogInformation("Database initialized (migrations applied, data seeded).");
    }
}
