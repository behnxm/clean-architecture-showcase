using CleanArchitectureShowcase.Application;
using CleanArchitectureShowcase.Infrastructure;
using CleanArchitectureShowcase.Infrastructure.Persistence;
using CleanArchitectureShowcase.WebAPI.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pipeline middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Clean Architecture Product Catalog API");
    });

    // In development, apply EF Core migrations and seed products
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    bool applyMigrations = builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true);
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
