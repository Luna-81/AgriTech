using Domain.Common.Interfaces;

namespace Application.Common.Interfaces;

/// <summary>
/// Domain event dispatcher interface for dispatching domain events to the appropriate handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}