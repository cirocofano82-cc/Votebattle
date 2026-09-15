using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure;

/// <summary>
/// Registers infrastructure services (database, and later Stripe/email/captcha) in DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        return services;
    }

    /// <summary>
    /// Resolves the PostgreSQL connection string. The DATABASE_CONNECTION_STRING
    /// environment variable takes precedence over the ConnectionStrings:Default setting.
    /// </summary>
    public static string ResolveConnectionString(IConfiguration configuration)
    {
        var fromEnv = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        var fromConfig = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return fromConfig;

        throw new InvalidOperationException(
            "No database connection string configured. Set DATABASE_CONNECTION_STRING " +
            "or ConnectionStrings:Default.");
    }
}
