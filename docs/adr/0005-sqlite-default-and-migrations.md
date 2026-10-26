# ADR 0005: SQLite Default and Migrations

## Status
Accepted

## Context
A showcase and interview reference repository must provide a frictionless "clone and run" developer experience. Requiring developers to install, configure, and run external database servers (e.g. Dockerized PostgreSQL or local SQL Server) raises the friction for quick reviews and automated CI pipelines.

## Decision
I selected **SQLite** as the default storage provider:
- Default connection string is `Data Source=productcatalog.db`, ensuring that `dotnet run --project src/CleanArchitectureShowcase.WebAPI` functions out of the box with zero external dependencies.
- Production-grade EF Core migrations are maintained and committed in `Infrastructure/Persistence/Migrations/`.
- In Development mode or when `Database:ApplyMigrationsOnStartup` is true, `DbInitializer` applies migrations and seeds 5 realistic products.
- **SQLite Decimal Ordering Workaround**: SQLite lacks a native decimal data type and stores decimals as TEXT, which causes `ORDER BY` queries on decimal columns to sort lexicographically rather than numerically (e.g. "100.0" sorts before "20.0"). To solve this while keeping decimal precision in domain entities, I configured a `decimal-to-double` EF Core value converter in `ApplicationDbContext.OnModelCreating` when `Database.IsSqlite()`.

## Consequences

### Positive
- **Instant Developer Experience**: Clone the repository and immediately execute `dotnet test` or `dotnet run` without docker-compose or server setup.
- **Seamless Portability**: Switching to PostgreSQL or SQL Server requires only:
  1. Installing the provider package (`Npgsql.EntityFrameworkCore.PostgreSQL` or `Microsoft.EntityFrameworkCore.SqlServer`).
  2. Updating the connection string in `appsettings.json`.
  3. Swapping `options.UseSqlite` to `options.UseNpgsql` or `options.UseSqlServer` in `Infrastructure/DependencyInjection.cs`.
  4. Removing the SQLite-specific decimal converter.
  5. Generating fresh migrations.

### Negative
- SQLite has known concurrency constraints (file-level write locks) that make it unsuitable for high-throughput write workloads in clustered environments.
