// src/Domain/Alerting/AlertResult.cs
namespace Domain.Alerting;

public class AlertResult
{
    public bool IsAlertTriggered { get; set; }
    public string? AlertMessage { get; set; }
    public AlertSeverity Severity { get; set; }
    public string? StrategyName { get; set; }
    public double? ThresholdValue { get; set; }
    public double? ActualValue { get; set; }

    public static AlertResult NoAlert() => new() { IsAlertTriggered = false };

    public static AlertResult Trigger(
        string message,
        AlertSeverity severity,
        string strategyName,
        double? threshold = null,
        double? actual = null) =>
        new()
        {
            IsAlertTriggered = true,
            AlertMessage = message,
            Severity = severity,
            StrategyName = strategyName,
            ThresholdValue = threshold,
            ActualValue = actual
        };
}