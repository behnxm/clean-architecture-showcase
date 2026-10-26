# Build stage using the official .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-wide build and central package management props first to optimize Docker layer caching
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/CleanArchitectureShowcase.Domain/CleanArchitectureShowcase.Domain.csproj src/CleanArchitectureShowcase.Domain/
COPY src/CleanArchitectureShowcase.Application/CleanArchitectureShowcase.Application.csproj src/CleanArchitectureShowcase.Application/
COPY src/CleanArchitectureShowcase.Infrastructure/CleanArchitectureShowcase.Infrastructure.csproj src/CleanArchitectureShowcase.Infrastructure/
COPY src/CleanArchitectureShowcase.WebAPI/CleanArchitectureShowcase.WebAPI.csproj src/CleanArchitectureShowcase.WebAPI/

# Restore dependencies
RUN dotnet restore src/CleanArchitectureShowcase.WebAPI/CleanArchitectureShowcase.WebAPI.csproj

# Copy full source tree
COPY src/ src/

# Build and publish application in Release configuration
RUN dotnet publish src/CleanArchitectureShowcase.WebAPI/CleanArchitectureShowcase.WebAPI.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# Runtime stage using the smallest official .NET 10 runtime variant (Alpine Linux)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Prepare SQLite storage directory with appropriate non-root user permissions
RUN mkdir -p /app/data && chown -R app:app /app/data

# Enforce least-privilege security by executing as non-root user
USER app

# Copy compiled application assets
COPY --from=build --chown=app:app /app/publish .

# Set default environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Database="Data Source=/app/data/productcatalog.db"

# Expose default HTTP container port
EXPOSE 8080

# Mount persistent volume for SQLite database storage
VOLUME /app/data

# Container healthcheck querying the liveness endpoint
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CleanArchitectureShowcase.WebAPI.dll"]
