# ADR 0004: Hand-Written Mapping Instead of AutoMapper

## Status
Accepted

## Context
AutoMapper has historically been used to copy property values between domain entities and presentation DTOs. However:
1. AutoMapper uses runtime reflection and expression compilation that impacts startup time and cold-path performance.
2. Silent property mismatches or schema refactorings can cause silent runtime nulls or exceptions rather than compile-time errors.
3. In late 2023, the creator of AutoMapper announced that AutoMapper would not be receiving further major architectural revisions and recommended hand-written mapping or compile-time generators for modern .NET applications.

## Decision
I adopted explicit, hand-written mapping extension methods (`ProductMappingExtensions.ToResponse()`).
- Projections from `Product` to `ProductResponse` are pure C# extension methods.
- Read queries in `ProductReadRepository` project directly to `ProductResponse` within LINQ `Select` expressions, allowing EF Core to generate targeted SQL queries that select only the needed columns.

## Consequences

### Positive
- **Compile-Time Safety**: Renaming or removing a property causes immediate compiler errors if mappings are out of sync.
- **Zero Allocation & Zero Reflection**: Completely avoids reflection overhead, configuration mapping profiles, and runtime proxy generation.
- **Debuggability**: Full stack traces and breakpoint debugging straight through the mapping logic.

### Negative
- Requires writing explicit mapping code for new DTOs. For larger enterprise models, code generators (e.g. Riok.Mapperly) can automate this at compile time.
