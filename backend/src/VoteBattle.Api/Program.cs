using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VoteBattle.Api.Controllers;
using VoteBattle.Api.Extensions;
using VoteBattle.Api.Middleware;
using VoteBattle.Core.Common;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Options;
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

// --- Rate limiting (per user id, or IP when anonymous) --------------------
var rl = new RateLimitOptions();
if (int.TryParse(builder.Configuration["RATELIMIT_AUTH_PER_MINUTE"], out var aRl)) rl.AuthPerMinute = aRl;
if (int.TryParse(builder.Configuration["RATELIMIT_VOTE_PER_MINUTE"], out var vRl)) rl.VotePerMinute = vRl;
if (int.TryParse(builder.Configuration["RATELIMIT_COMMENT_PER_MINUTE"], out var cRl)) rl.CommentPerMinute = cRl;
if (int.TryParse(builder.Configuration["RATELIMIT_CHECKOUT_PER_MINUTE"], out var koRl)) rl.CheckoutPerMinute = koRl;
if (int.TryParse(builder.Configuration["RATELIMIT_WEBHOOK_PER_MINUTE"], out var wRl)) rl.WebhookPerMinute = wRl;

static string PartitionKey(HttpContext ctx) =>
    ctx.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
    ?? ctx.Connection.RemoteIpAddress?.ToString()
    ?? "anonymous";

RateLimitPartition<string> FixedWindow(HttpContext ctx, int perMinute) =>
    RateLimitPartition.GetFixedWindowLimiter(PartitionKey(ctx), _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = perMinute,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", ctx => FixedWindow(ctx, rl.AuthPerMinute));
    options.AddPolicy("vote", ctx => FixedWindow(ctx, rl.VotePerMinute));
    options.AddPolicy("comment", ctx => FixedWindow(ctx, rl.CommentPerMinute));
    options.AddPolicy("checkout", ctx => FixedWindow(ctx, rl.CheckoutPerMinute));
    options.AddPolicy("webhook", ctx => FixedWindow(ctx, rl.WebhookPerMinute));

    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();

        context.HttpContext.Response.ContentType = "application/json";
        var body = ApiResponse.Fail("Too many requests. Please slow down and try again shortly.");
        await context.HttpContext.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            token);
    };
});

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

// In production, CAPTCHA must be enabled.
if (!app.Environment.IsDevelopment() &&
    !string.Equals(app.Configuration["TURNSTILE_ENABLED"], "true", StringComparison.OrdinalIgnoreCase))
{
    app.Logger.LogWarning(
        "TURNSTILE_ENABLED is not 'true' in a non-Development environment. " +
        "CAPTCHA protection is DISABLED — set TURNSTILE_ENABLED=true for production.");
}

// --- Middleware pipeline --------------------------------------------------

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Basic security headers.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// Exposed for integration tests (WebApplicationFactory<Program>).
public partial class Program { }
