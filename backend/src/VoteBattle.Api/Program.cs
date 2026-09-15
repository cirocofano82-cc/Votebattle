using Microsoft.AspNetCore.Identity;
using VoteBattle.Core.Entities;
using VoteBattle.Infrastructure;
using VoteBattle.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Services -------------------------------------------------------------

// Database (PostgreSQL via EF Core) + infrastructure services.
builder.Services.AddInfrastructure(builder.Configuration);

// Data protection is required by Identity's default token providers
// (email confirmation, password reset tokens).
builder.Services.AddDataProtection();

// ASP.NET Core Identity (users + roles). Auth flows are wired in a later step.
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
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

// --- Middleware pipeline --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Simple liveness endpoint.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
