// src/Application/Events/AlertTriggeredEvent.cs
namespace Application.Events;

public class AlertTriggeredEvent
{
    public Guid AlertId { get; set; }
    public Guid SensorId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public double ThresholdValue { get; set; }
    public AlertSeverity Severity { get; set; }
    public DateTime TriggeredAt { get; set; }
}