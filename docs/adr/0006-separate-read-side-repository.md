# ADR 0006: Separate Read-Side Repository

## Status
Accepted

## Context
Traditional Clean Architecture implementations often force both read and write operations through a single repository abstraction (e.g. `IRepository<T>`). In this anti-pattern:
1. Queries retrieve full domain aggregates, incurring unnecessary change-tracking overhead (`ChangeTracker`).
2. Read operations load entire aggregate graphs, including private collections and state, only to map a subset of properties to a DTO.
3. Complex queries (filtering, sorting, paging) force domain models to expose internal structures or load unnecessary data into application memory.

## Decision
I separated read and write operations into distinct abstractions following **Command Query Responsibility Segregation (CQRS)**:
- **Write-Side Repository (`IProductRepository` in Domain)**: Manages the `Product` aggregate root with tracked state transitions, entity encapsulation, and invariant enforcement. Only contains methods needed for write mutations (`GetByIdAsync`, `AddAsync`, `Remove`, `ExistsByNameAsync`).
- **Read-Side Repository (`IProductReadRepository` in Application)**: Tailored exclusively for querying. Uses `AsNoTracking()`, server-side SQL projection directly to `ProductResponse` DTOs, and server-side filtering, sorting, and pagination.

## Consequences

### Positive
- **Optimal Read Performance**: Queries avoid EF Core change tracking, minimizing memory allocations and garbage collection pressure.
- **SQL Efficiency**: EF Core generates `SELECT` statements containing only the specific columns requested by the `ProductResponse` projection.
- **Domain Encapsulation**: Domain entities never need public getters or leaked internal structures solely to satisfy reporting or query needs.

### Negative
- Requires maintaining two repository interfaces and implementations for the aggregate.
