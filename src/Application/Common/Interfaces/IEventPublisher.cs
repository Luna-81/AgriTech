namespace Application.Common.Interfaces;

/// <summary>
/// event publisher interface for publishing events to the event bus.
/// implementations of this interface can be used to publish events to the event bus, such as RabbitMQ or Kafka.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class;
}