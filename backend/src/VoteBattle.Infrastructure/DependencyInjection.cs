using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VoteBattle.Core.Interfaces;
using VoteBattle.Core.Options;
using VoteBattle.Infrastructure.Captcha;
using VoteBattle.Infrastructure.Data;
using VoteBattle.Infrastructure.Email;
using VoteBattle.Infrastructure.Services;

namespace VoteBattle.Infrastructure;

/// <summary>
/// Registers infrastructure services (database, options, email, captcha, domain services).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database (PostgreSQL via EF Core).
        var connectionString = ResolveConnectionString(configuration);
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Options (bound from configuration / environment variables).
        services.Configure<AuthOptions>(o =>
        {
            o.FrontendUrl = configuration["FrontendUrl"]
                ?? configuration["FRONTEND_URL"]
                ?? o.FrontendUrl;
        });

        services.Configure<TurnstileOptions>(o =>
        {
            o.Enabled = bool.TryParse(configuration["TURNSTILE_ENABLED"], out var enabled) && enabled;
            o.SiteKey = configuration["TURNSTILE_SITE_KEY"] ?? string.Empty;
            o.SecretKey = configuration["TURNSTILE_SECRET_KEY"] ?? string.Empty;
        });

        services.Configure<EmailOptions>(o =>
        {
            o.FromAddress = configuration["EMAIL_FROM_ADDRESS"] ?? o.FromAddress;
            o.FromName = configuration["EMAIL_FROM_NAME"] ?? o.FromName;
            o.SmtpHost = configuration["SMTP_HOST"] ?? o.SmtpHost;
            o.SmtpPort = int.TryParse(configuration["SMTP_PORT"], out var port) ? port : o.SmtpPort;
            o.Username = configuration["SMTP_USERNAME"];
            o.Password = configuration["SMTP_PASSWORD"];
            o.UseSsl = bool.TryParse(configuration["SMTP_USE_SSL"], out var ssl) && ssl;
        });

        // Domain services.
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBattleService, BattleService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // Typed HttpClient for Cloudflare Turnstile verification.
        services.AddHttpClient<ICaptchaService, TurnstileCaptchaService>();

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
