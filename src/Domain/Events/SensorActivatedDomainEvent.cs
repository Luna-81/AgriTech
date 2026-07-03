using MediatR;

namespace Domain.Events;

/// <summary>
/// 传感器激活领域事件
/// 当传感器被激活时触发
/// </summary>
public class SensorActivatedDomainEvent : INotification
{
    public Guid SensorId { get; }
    public DateTime ActivatedAt { get; }

    public SensorActivatedDomainEvent(Guid sensorId, DateTime activatedAt)
    {
        SensorId = sensorId;
        ActivatedAt = activatedAt;
    }
}