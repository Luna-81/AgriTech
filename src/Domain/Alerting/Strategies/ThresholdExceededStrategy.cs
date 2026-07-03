// src/Domain/Alerting/Strategies/ThresholdExceededStrategy.cs
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;

namespace Domain.Alerting.Strategies;

public class ThresholdExceededStrategy : IAlertStrategy
{
    private readonly double _temperatureThreshold;
    private readonly double _humidityThreshold;

    public string StrategyName => nameof(ThresholdExceededStrategy);
    public AlertSeverity Severity => AlertSeverity.Error;

    public ThresholdExceededStrategy(
        double temperatureThreshold = 40.0,
        double humidityThreshold = 85.0)
    {
        _temperatureThreshold = temperatureThreshold;
        _humidityThreshold = humidityThreshold;
    }

    public Task<AlertResult> EvaluateAsync(
        SensorReading reading,
        AlertContext context,
        CancellationToken cancellationToken = default)
    {
        if (reading.Temperature.Celsius >= _temperatureThreshold)
        {
            return Task.FromResult(AlertResult.Trigger(
                $"温度 {reading.Temperature.Celsius:F1}°C 超过阈值 {_temperatureThreshold}°C",
                AlertSeverity.Error,
                StrategyName,
                _temperatureThreshold,
                reading.Temperature.Celsius
            ));
        }

        if (reading.Humidity.Percentage >= _humidityThreshold)
        {
            return Task.FromResult(AlertResult.Trigger(
                $"湿度 {reading.Humidity.Percentage:F1}% 超过阈值 {_humidityThreshold}%",
                AlertSeverity.Error,
                StrategyName,
                _humidityThreshold,
                reading.Humidity.Percentage
            ));
        }

        return Task.FromResult(AlertResult.NoAlert());
    }
}