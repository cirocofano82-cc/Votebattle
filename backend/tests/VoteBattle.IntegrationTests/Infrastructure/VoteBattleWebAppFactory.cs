using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API for integration tests. Uses the test database (via the
/// DATABASE_CONNECTION_STRING env var set by the test run) and a fake email service.
/// </summary>
public class VoteBattleWebAppFactory : WebApplicationFactory<Program>
{
    public const string WebhookSecret = "whsec_test_secret_123";

    public FakeEmailService Email { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(Email);
        });
    }
}
