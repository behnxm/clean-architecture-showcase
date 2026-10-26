using CleanArchitectureShowcase.Domain.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureShowcase.Application.Common.Events;

/// <summary>
/// Domain event handler demonstrating asynchronous reactivity to aggregate creation.
/// Uses high-performance source-generated LoggerMessage logging.
/// </summary>
internal sealed partial class ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger)
    : IDomainEventHandler<ProductCreatedDomainEvent>
{
    public Task Handle(ProductCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        LogProductCreated(logger, domainEvent.ProductId, domainEvent.Name, domainEvent.Price);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Domain event handled: Product created with ID '{ProductId}', Name '{Name}', Price {Price}")]
    private static partial void LogProductCreated(ILogger logger, Guid productId, string name, decimal price);
}
