// src/Application/Events/SensorReadingRecordedEvent.cs
namespace Application.Events;

/// <summary>
/// 传感器读数记录事件
/// 当传感器上报新数据时发布
/// </summary>
public class SensorReadingRecordedEvent
{
    public Guid SensorId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public DateTime Timestamp { get; set; }
}