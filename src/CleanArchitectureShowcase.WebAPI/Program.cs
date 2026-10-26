using CleanArchitectureShowcase.Application;
using CleanArchitectureShowcase.Infrastructure;
using CleanArchitectureShowcase.Infrastructure.Persistence;
using CleanArchitectureShowcase.WebAPI.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Presentation and infrastructure services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS policy configuration
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000"];

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

WebApplication app = builder.Build();

// Pipeline middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

bool enableOpenApi = app.Environment.IsDevelopment() || builder.Configuration.GetValue("OpenApi:Enabled", true);
if (enableOpenApi)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Clean Architecture Product Catalog API"));
}

// In Development, or when configured via Database:ApplyMigrationsOnStartup, apply migrations and seed products
bool applyMigrations = builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true);
if (app.Environment.IsDevelopment() || applyMigrations)
{
    using IServiceScope scope = app.Services.CreateScope();
    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbInitializer.InitializeAsync(dbContext, applyMigrations, logger);
}

app.UseCors("Frontend");

app.MapControllers();

// Health checks:
// /health (Liveness): responds 200 OK immediately when the web server process is responsive
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

// /health/ready (Readiness): verifies database context connectivity
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database") || true
});

await app.RunAsync();

/// <summary>
/// Partial Program class definition required for WebApplicationFactory integration testing.
/// </summary>
public partial class Program;
