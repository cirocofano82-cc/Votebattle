using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VoteBattle.Api.Controllers;
using VoteBattle.Api.Extensions;
using VoteBattle.Api.Middleware;
using VoteBattle.Core.Common;
using VoteBattle.Core.Entities;
using VoteBattle.Infrastructure;
using VoteBattle.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Services -------------------------------------------------------------

// Database (PostgreSQL via EF Core) + infrastructure services.
builder.Services.AddInfrastructure(builder.Configuration);

// Data protection is required by Identity's default token providers and the auth cookie.
builder.Services.AddDataProtection();

// ASP.NET Core Identity with cookie authentication (HttpOnly session cookie).
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;

        options.SignIn.RequireConfirmedEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure the Identity application cookie for an SPA/API (no redirects).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "votebattle.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS in production
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    // Return status codes instead of redirecting to login/access-denied pages.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Uniform validation error responses ({ success, message, errors }).
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        return new BadRequestObjectResult(ApiResponse.Fail("Validation failed.", errors));
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS so the Next.js frontend (a different origin) can call the API with cookies.
const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"]
            ?? Environment.GetEnvironmentVariable("FRONTEND_URL")
            ?? "http://localhost:3000";

        policy.WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Apply migrations and seed baseline data at startup.
await app.InitializeDatabaseAsync();

// --- Middleware pipeline --------------------------------------------------

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// Exposed for integration tests (WebApplicationFactory<Program>).
public partial class Program { }
