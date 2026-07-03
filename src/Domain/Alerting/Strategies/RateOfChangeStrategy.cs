// src/Domain/Alerting/Strategies/RateOfChangeStrategy.cs
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;

namespace Domain.Alerting.Strategies;

public class RateOfChangeStrategy : IAlertStrategy
{
    private readonly double _maxRatePerSecond;

    public string StrategyName => nameof(RateOfChangeStrategy);
    public AlertSeverity Severity => AlertSeverity.Critical;

    public RateOfChangeStrategy(double maxRatePerSecond = 0.5)
    {
        _maxRatePerSecond = maxRatePerSecond;
    }

    public Task<AlertResult> EvaluateAsync(
        SensorReading reading,
        AlertContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.PreviousTemperature.HasValue || !context.PreviousTemperatureTime.HasValue)
        {
            context.PreviousTemperature = reading.Temperature.Celsius;
            context.PreviousTemperatureTime = reading.Timestamp;
            return Task.FromResult(AlertResult.NoAlert());
        }

        var timeDelta = (reading.Timestamp - context.PreviousTemperatureTime.Value).TotalSeconds;
        if (timeDelta <= 0)
            return Task.FromResult(AlertResult.NoAlert());

        var tempDelta = Math.Abs(reading.Temperature.Celsius - context.PreviousTemperature.Value);
        var rate = tempDelta / timeDelta;

        context.PreviousTemperature = reading.Temperature.Celsius;
        context.PreviousTemperatureTime = reading.Timestamp;

        if (rate > _maxRatePerSecond)
        {
            return Task.FromResult(AlertResult.Trigger(
                $"温度变化率异常：{rate:F2}°C/秒（阈值 {_maxRatePerSecond}°C/秒）",
                AlertSeverity.Critical,
                StrategyName,
                _maxRatePerSecond,
                rate
            ));
        }

        return Task.FromResult(AlertResult.NoAlert());
    }
}