# Technical Interview Notes: Clean Architecture Showcase

This guide contains 12 in-depth architectural questions and detailed answers about this codebase. Every answer references concrete files, design decisions, and trade-offs.

---

### 1. What is Clean Architecture, and how does this repository enforce the Dependency Rule?
**Answer:**
Clean Architecture structures software into concentric layers where dependencies point strictly inward toward high-level business policies:
- **Domain** (`src/CleanArchitectureShowcase.Domain`): Enterprise business rules and aggregates. It has **zero** package dependencies and zero framework references.
- **Application** (`src/CleanArchitectureShowcase.Application`): Use-case orchestrators (commands, queries, pipeline behaviors, validators). It depends exclusively on Domain.
- **Infrastructure** (`src/CleanArchitectureShowcase.Infrastructure`): Concrete implementations of persistence (EF Core SQLite), external adapters, and interceptors. It depends on Application and Domain.
- **WebAPI** (`src/CleanArchitectureShowcase.WebAPI`): Composition root and presentation layer (controllers, OpenAPI, Scalar). It depends on Application and Infrastructure.

To guarantee that developers cannot inadvertently violate these boundaries, automated architectural tests in [`CleanArchitectureTests.cs`](../tests/CleanArchitectureShowcase.ArchitectureTests/CleanArchitectureTests.cs) inspect compiled assemblies with NetArchTest. If Domain references any other layer or third-party package, or if Application references EF Core or Infrastructure, the build pipeline fails immediately.

---

### 2. Why did you write your own CQRS dispatcher instead of using MediatR?
**Answer:**
I replaced MediatR with a hand-written engine in [`Dispatcher.cs`](../src/CleanArchitectureShowcase.Application/Common/Messaging/Dispatcher.cs) for two reasons:
1. **Licensing Risks**: In early 2024, MediatR transitioned from Apache-2.0 to a dual commercial / Reciprocal Public License 1.5 (RPL-1.5) model starting with version 13.0. In modern enterprise environments, dependencies must carry permissive licenses (MIT, Apache-2.0, BSD) to eliminate legal compliance risks.
2. **Transparency and Architectural Mastery**: Standard mediator packages often obscure the underlying machinery. Writing the mediator from scratch demonstrates that a full-featured in-process CQRS pipeline requires only about 100 lines of clear C# using core design patterns: Mediator, Decorator, and Chain of Responsibility.

---

### 3. How does your Dispatcher eliminate runtime reflection overhead?
**Answer:**
In [`Dispatcher.cs`](../src/CleanArchitectureShowcase.Application/Common/Messaging/Dispatcher.cs), I implemented a **generic cached invoker pattern**.
- When a command or query is dispatched, the dispatcher checks a thread-safe `ConcurrentDictionary<Type, object>` for an invoker keyed by the request's runtime type.
- On the first call for a new request type, it uses `Type.MakeGenericType` once to instantiate a specialized generic invoker (e.g., `CommandInvokerImpl<TCommand>`) that holds strongly typed references.
- On all subsequent calls, the invoker is retrieved from the dictionary and executed statically without reflection, expression trees, or boxing.
- The pipeline chain is assembled with a right-to-left fold so that the first registered behavior (e.g. `LoggingBehavior`) acts as the outermost decorator, wrapping inner behaviors and the terminal handler.

---

### 4. Why use the Result pattern instead of throwing domain exceptions?
**Answer:**
In [`Result.cs`](../src/CleanArchitectureShowcase.Domain/Common/Result.cs) and [`Error.cs`](../src/CleanArchitectureShowcase.Domain/Common/Error.cs), I replaced domain exceptions with the Result pattern for two primary reasons:
1. **Performance**: Capturing stack traces and allocating exception objects in .NET is computationally expensive. For expected domain branches (e.g. invalid inventory stock or duplicate names), exceptions represent unnecessary performance overhead.
2. **Deterministic Control Flow**: Returning `Result<T>` forces callers to explicitly handle success and failure paths, preventing hidden unhandled exceptions. In [`ResultExtensions.cs`](../src/CleanArchitectureShowcase.WebAPI/Extensions/ResultExtensions.cs), failed results are deterministically mapped to RFC 9457 ProblemDetails with HTTP status codes 400, 404, or 409 based on the `ErrorType`.

---

### 5. How does your ValidationBehavior instantiate typed failure results without reflection?
**Answer:**
A recurring problem with generic pipeline behaviors in C# is: how does a behavior like `ValidationBehavior<TRequest, TResponse>` return a failure of type `TResponse` when `TResponse` is constrained only to `Result` or `Result<T>`?
In this codebase, I used C# static abstract interface members:
- Domain defines [`IFailureResult<TSelf>`](../src/CleanArchitectureShowcase.Domain/Common/IFailureResult.cs) with `static abstract TSelf Failure(Error error)`.
- Both `Result` and `Result<TValue>` implement `IFailureResult`.
- Inside [`ValidationBehavior.cs`](../src/CleanArchitectureShowcase.Application/Common/Behaviors/ValidationBehavior.cs), a cached generic factory creates a typed delegate once per `TResponse` type. When validation errors occur, it creates the failed response with zero runtime reflection.

---

### 6. How did you implement CQRS, and why are there separate repositories for read and write?
**Answer:**
I separated read and write paths into distinct abstractions:
- **Write-Side (`IProductRepository`)**: Declared in Domain ([`IProductRepository.cs`](../src/CleanArchitectureShowcase.Domain/Abstractions/IProductRepository.cs)) and implemented in Infrastructure ([`ProductRepository.cs`](../src/CleanArchitectureShowcase.Infrastructure/Persistence/Repositories/ProductRepository.cs)). It manages the `Product` aggregate root with full entity encapsulation and change tracking.
- **Read-Side (`IProductReadRepository`)**: Declared in Application ([`IProductReadRepository.cs`](../src/CleanArchitectureShowcase.Application/Abstractions/IProductReadRepository.cs)) and implemented in Infrastructure ([`ProductReadRepository.cs`](../src/CleanArchitectureShowcase.Infrastructure/Persistence/Repositories/ProductReadRepository.cs)). Queries bypass domain aggregates and change tracking entirely (`AsNoTracking()`), projecting directly to `ProductResponse` DTOs in SQL.

This prevents the common anti-pattern where read operations pull heavy aggregate entities into memory just to map them to DTOs.

---

### 7. How are domain events handled, and what are the trade-offs of your event dispatching strategy?
**Answer:**
Domain events (such as [`ProductCreatedDomainEvent.cs`](../src/CleanArchitectureShowcase.Domain/Events/ProductCreatedDomainEvent.cs)) are recorded in-memory on the aggregate root using `AddDomainEvent`.
- In Infrastructure, [`DispatchDomainEventsInterceptor.cs`](../src/CleanArchitectureShowcase.Infrastructure/Persistence/Interceptors/DispatchDomainEventsInterceptor.cs) intercepts EF Core's `SavedChangesAsync`.
- **Trade-off**: Events are dispatched **after** the database transaction successfully commits. This ensures at-most-once delivery and prevents dispatching events for transactions that subsequently roll back.
- **Limitation & Outbox Pattern**: If the process crashes immediately after `SaveChangesAsync` but before the event handler finishes, the event is lost. In distributed systems requiring guaranteed at-least-once delivery, an Outbox table should be used to persist event messages within the same database transaction.

---

### 8. How does this codebase support testing of time-dependent logic?
**Answer:**
I completely avoided static calls like `DateTime.UtcNow`.
- The Infrastructure layer injects the modern .NET [`TimeProvider`](https://learn.microsoft.com/dotnet/api/system.timeprovider) abstraction into [`AuditableEntityInterceptor.cs`](../src/CleanArchitectureShowcase.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs).
- In production, `TimeProvider.System` is registered.
- In unit tests ([`AuditableEntityInterceptorTests.cs`](../tests/CleanArchitectureShowcase.UnitTests/Infrastructure/AuditableEntityInterceptorTests.cs)), I inject `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`, allowing tests to simulate elapsed time deterministically without fragile `Thread.Sleep` or system clock manipulation.

---

### 9. What is the SQLite decimal ordering limitation, and how did you resolve it?
**Answer:**
SQLite does not possess a native decimal data type. By default, EF Core stores `decimal` properties as TEXT strings. When executing `ORDER BY Price`, SQLite sorts lexicographically (e.g. `"100.0"` sorts before `"20.0"`).
- To solve this, in [`ApplicationDbContext.cs`](../src/CleanArchitectureShowcase.Infrastructure/Persistence/ApplicationDbContext.cs), I added an entity model configuration that detects when `Database.IsSqlite()` is active and registers a `HasConversion<double>()` converter across all decimal properties.
- This stores values as IEEE 754 floating-point numbers in SQLite, ensuring correct numeric sorting while preserving exact decimal precision on the C# domain entity.
- The behavior is explicitly verified by integration tests in [`ProductQueryTests.cs`](../tests/CleanArchitectureShowcase.IntegrationTests/Endpoints/ProductQueryTests.cs).

---

### 10. How do you ensure consistent RFC 9457 ProblemDetails across all HTTP failure paths?
**Answer:**
API errors originate from two sources:
1. **Expected Business/Validation Failures**: Handled via [`ResultExtensions.ToProblem()`](../src/CleanArchitectureShowcase.WebAPI/Extensions/ResultExtensions.cs). It maps `ValidationError` to 400 Bad Request (including an `errors` dictionary of field names to error messages), `NotFound` to 404, and `Conflict` to 409, embedding correlation `traceId` and error `code` extensions.
2. **Unhandled Runtime Exceptions**: Caught by [`GlobalExceptionHandler.cs`](../src/CleanArchitectureShowcase.WebAPI/Middleware/GlobalExceptionHandler.cs) implementing `IExceptionHandler`. It logs the exception with source-generated structured logging and emits a 500 ProblemDetails payload, sanitizing stack traces in non-development environments.

---

### 11. How are architectural rules and code conventions automatically verified?
**Answer:**
Automated architecture tests in [`CleanArchitectureTests.cs`](../tests/CleanArchitectureShowcase.ArchitectureTests/CleanArchitectureTests.cs) run as part of every build and CI pipeline:
- **Dependency Rule**: Asserts that Domain and Application never reference outer layers or forbidden packages.
- **Naming & Sealing**: Asserts that every command/query handler is sealed and ends with `Handler`, and all validators and pipeline behaviors are sealed.
- **Encapsulation**: Asserts that all aggregate entities have no public property setters.
- **Compiler Warnings**: In `Directory.Build.props`, `TreatWarningsAsErrors` is set to `true`, and code style analyzers run during build (`EnforceCodeStyleInBuild`).

---

### 12. When is Clean Architecture overkill, and what are the alternatives?
**Answer:**
Clean Architecture is designed for complex enterprise domains with rich business invariants, multiple integration boundaries, and long operational lifespans. It is **overkill** for:
- Simple CRUD applications where business logic consists only of storing and retrieving rows.
- Prototypes, MVPs, and short-lived internal microservices.

In such cases, Clean Architecture introduces unnecessary indirection (multiple DTOs, interfaces, and layer mappings).
**Alternatives**:
- **Vertical Slice Architecture**: Slices the application by feature rather than layer (e.g. each feature has its own endpoint, command, handler, and EF Core query in a single folder). This reduces cross-layer ceremonies while keeping use cases isolated.
- **Direct EF Core DbContext in Endpoints**: For plain CRUD endpoints, querying `DbContext` directly in Minimal API endpoints is often simpler and faster to maintain.
