# ADR 0002: Result Pattern Instead of Exceptions

## Status
Accepted

## Context
In traditional object-oriented applications, business rule violations and validation failures are often handled by throwing domain exceptions (e.g., `ProductNotFoundException`, `ValidationException`). However, exceptions in .NET:
1. Carry significant runtime cost (stack unwinding and allocation of exception objects).
2. Create implicit, non-deterministic control flow (a caller cannot tell from a method signature whether an exception will be thrown).
3. Conflate unexpected exceptional crashes (network partitions, out-of-memory errors) with expected domain outcomes (invalid stock count, duplicate product title).

## Decision
I implemented a robust **Result Pattern** with `Result` and `Result<T>` in the Domain layer (`Domain.Common`).
- Methods that can fail return `Result` or `Result<T>` instead of throwing.
- Failures carry a strongly typed `Error` record specifying an error code, description, and categorized `ErrorType` (`Failure`, `Validation`, `NotFound`, `Conflict`).
- Multiple validation failures are grouped into a `ValidationError` record.
- I introduced the C# static-abstract interface `IFailureResult<TSelf>` to permit pipeline behaviors (`ValidationBehavior`) to instantiate typed failure outcomes with zero reflection.
- Truly exceptional runtime faults (e.g. database disconnects or unhandled bugs) still throw exceptions and are caught at the presentation boundary by `GlobalExceptionHandler`.

## Consequences

### Positive
- **Explicit Contracts**: Return types communicate all possible outcomes directly in method signatures.
- **Performance**: Eliminates expensive stack frame captures for expected business branches.
- **Clean HTTP Mapping**: `ResultExtensions.ToProblem()` maps `ErrorType` directly to standard RFC 9457 HTTP ProblemDetails status codes (400, 404, 409, 500).

### Negative
- Callers must inspect `result.IsSuccess` or `result.IsFailure` before accessing `result.Value`.
- Accessing `Value` on a failed result throws `InvalidOperationException` to prevent reading uninitialized state.
