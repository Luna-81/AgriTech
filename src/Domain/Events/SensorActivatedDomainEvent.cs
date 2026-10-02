using Domain.Common.Interfaces;

namespace Domain.Events;

/// <summary>
/// sensor activated domain event that occurs when a sensor is activated.
/// When a sensor is activated, this event is triggered.
/// </summary>
public class SensorActivatedDomainEvent : IDomainEvent
{
    public Guid SensorId { get; }
    public DateTime ActivatedAt { get; }
    public DateTime OccurredAt => ActivatedAt;

    public SensorActivatedDomainEvent(Guid sensorId, DateTime activatedAt)
    {
        SensorId = sensorId;
        ActivatedAt = activatedAt;
    }
}