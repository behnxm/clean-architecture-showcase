# ADR 0001: Clean Architecture and the Dependency Rule

## Status
Accepted

## Context
When designing enterprise .NET applications, software systems frequently suffer from architectural erosion: presentation concerns leak into domain logic, ORM and database dependencies dictate business models, and testing requires complex mocking of framework infrastructure. I needed an architectural blueprint that enforces separation of concerns, guarantees testability, and insulates core business rules from external technology churn.

## Decision
I adopted Clean Architecture (also known as Onion or Hexagonal Architecture), organized into four distinct rings:
1. **Domain**: Pure enterprise business rules and entities (`BaseEntity<T>`, `Product`), value objects, and domain repository interfaces (`IProductRepository`, `IUnitOfWork`). This project has zero NuGet dependencies and zero knowledge of external frameworks.
2. **Application**: Use cases, command and query handlers, validation rules, and the mediator pipeline. Depends strictly on Domain.
3. **Infrastructure**: Framework and data access implementations (EF Core SQLite, interceptors, repository implementations). Implements interfaces defined in Domain and Application.
4. **WebAPI**: Presentation entry point and composition root. Exposes HTTP REST endpoints, configures middleware, and maps incoming requests to application commands and queries.

The primary guiding constraint is the **Dependency Rule**: source code dependencies must point exclusively inward toward high-level policies. The Domain knows nothing of the Application; the Application knows nothing of Infrastructure or WebAPI.

## Consequences

### Positive
- **Independent of Frameworks**: Business logic does not depend on EF Core, ASP.NET Core, or third-party libraries.
- **Independent of UI**: The Web API layer can be swapped for a gRPC service or CLI without changing business logic.
- **Independent of Database**: Switching from SQLite to PostgreSQL or SQL Server is confined to Infrastructure.
- **High Testability**: Core rules and use cases are thoroughly testable without spin-up of databases or web servers.

### Negative
- **Indirection**: Requires distinct DTOs and mapping boundaries between Web API requests, domain aggregates, and query responses.
- **Initial Setup**: More boilerplate upfront compared to a single-project vertical slice application.
