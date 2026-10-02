using Application.Common.Interfaces;
using MassTransit;

namespace Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IBus _bus;

    public MassTransitEventPublisher(IBus bus) => _bus = bus;

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        await _bus.Publish(@event, ct);
    }
}