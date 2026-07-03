// src/Domain/Alerting/IAlertStrategy.cs
using Domain.Sensors.Entities;
namespace Domain.Alerting;

public interface IAlertStrategy
{
    string StrategyName { get; }
    AlertSeverity Severity { get; }
    Task<AlertResult> EvaluateAsync(
        SensorReading reading,
        AlertContext context,
        CancellationToken cancellationToken = default);
}