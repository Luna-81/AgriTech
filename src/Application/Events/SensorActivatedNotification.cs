using MediatR;

namespace Application.Events;

public class SensorActivatedNotification : INotification
{
    public Guid SensorId { get; }
    public DateTime ActivatedAt { get; }

    public SensorActivatedNotification(Guid sensorId, DateTime activatedAt)
    {
        SensorId = sensorId;
        ActivatedAt = activatedAt;
    }
}