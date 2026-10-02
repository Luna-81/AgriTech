using Application.Common.Interfaces;
using Application.Events;
using Domain.Common.Interfaces;
using Domain.Events;
using MediatR;

namespace Application.Common.Dispatchers;

/// <summary>
/// Maps domain events to Application notifications and dispatches them via MediatR.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator) => _mediator = mediator;

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        INotification notification = domainEvent switch
        {
            SensorActivatedDomainEvent e => new SensorActivatedNotification(e.SensorId, e.ActivatedAt),
            _ => throw new NotSupportedException($"Unknown domain event: {domainEvent.GetType().Name}")
        };

        await _mediator.Publish(notification, ct);
    }
}