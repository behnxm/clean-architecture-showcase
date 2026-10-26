# ADR 0003: Hand-Written Dispatcher Instead of MediatR

## Status
Accepted

## Context
MediatR has historically been the de facto in-process mediator library for .NET CQRS architectures. However:
1. **Licensing Shifts**: Starting with MediatR 13+, the package adopted a dual-license model under the Reciprocal Public License 1.5 (RPL-1.5) and commercial licensing, making it unsuitable for zero-license-risk enterprise projects.
2. **Hidden Machinery**: Junior and intermediate developers often treat MediatR as "magic", obscuring how dependency injection, decorators, and invocation pipelines actually work.
3. **Overhead**: Standard mediator implementations rely on runtime dynamic reflection or unbounded expression compiling on every request.

## Decision
I designed and implemented a **zero-dependency, hand-written CQRS and pipeline engine** in `Application.Common.Messaging`:
- Implements `ICommand`, `ICommand<TResponse>`, `IQuery<TResponse>`, and matching handler abstractions.
- Implements `IPipelineBehavior<TRequest, TResponse>` supporting cross-cutting decorators (logging, validation).
- Uses a **generic cached invoker pattern**: upon first dispatch of a request type, an invoker is instantiated once via reflection and cached in a thread-safe `ConcurrentDictionary<Type, object>`. All subsequent calls are dispatched via statically typed delegates.
- Assembles the behavior pipeline using a right-to-left fold so that the first registered behavior is the outermost executing decorator.

## Consequences

### Positive
- **Zero Third-Party Dependencies**: No external package reference required; 100% auditable and MIT-licensed.
- **High Performance**: Statically typed execution after a single initial invocation; no recurring reflection overhead.
- **Educational Value**: Clear, transparent demonstration of the Mediator, Decorator, and Chain of Responsibility patterns in under 120 lines of core logic.

### Negative
- Requires maintaining the dispatcher codebase directly rather than consuming an external library.
- For Native AOT scenarios where runtime reflection is entirely prohibited, a compile-time source-generated alternative (e.g., `martinothamar/Mediator`) would be preferred.
