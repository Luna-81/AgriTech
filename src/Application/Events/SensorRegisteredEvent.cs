// src/Application/Events/SensorRegisteredEvent.cs
namespace Application.Events;

/// <summary>
/// 传感器注册事件 - 发布到 RabbitMQ
/// </summary>
public class SensorRegisteredEvent
{
    public Guid SensorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double TemperatureThreshold { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid FarmId { get; set; }
    public DateTime RegisteredAt { get; set; }
}