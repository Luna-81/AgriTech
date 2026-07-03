// src/Application/Events/SensorActivatedIntegrationEvent.cs
namespace Application.Events;

/// <summary>
/// 传感器激活集成事件
/// </summary>
public class SensorActivatedIntegrationEvent
{
    public Guid SensorId { get; set; }
    public DateTime ActivatedAt { get; set; }
}