// src/Domain/Alerting/AlertContext.cs
using Domain.Sensors.Entities;
using Domain.Sensors.Enums;

namespace Domain.Alerting;

public class AlertContext
{
    public Guid SensorId { get; set; }
    public SensorStatus SensorStatus { get; set; }
    public List<SensorReading> RecentReadings { get; set; } = new();
    public Dictionary<string, object> State { get; set; } = new();

    // 连续超标计数
    public int ConsecutiveExceedCount { get; set; }
    public DateTime? LastExceedTime { get; set; }

    // 变化率检测
    public double? PreviousTemperature { get; set; }
    public DateTime? PreviousTemperatureTime { get; set; }

    public void ResetConsecutiveCount() => ConsecutiveExceedCount = 0;
}