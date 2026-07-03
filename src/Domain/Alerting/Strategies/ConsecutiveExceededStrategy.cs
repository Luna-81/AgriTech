// src/Domain/Alerting/Strategies/ConsecutiveExceededStrategy.cs
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;

namespace Domain.Alerting.Strategies;

public class ConsecutiveExceededStrategy : IAlertStrategy
{
    private readonly int _requiredConsecutiveCount;
    private readonly double _temperatureThreshold;
    private readonly int _timeWindowMinutes;

    public string StrategyName => nameof(ConsecutiveExceededStrategy);
    public AlertSeverity Severity => AlertSeverity.Warning;

    public ConsecutiveExceededStrategy(
        int requiredConsecutiveCount = 3,
        double temperatureThreshold = 38.0,
        int timeWindowMinutes = 30)
    {
        _requiredConsecutiveCount = requiredConsecutiveCount;
        _temperatureThreshold = temperatureThreshold;
        _timeWindowMinutes = timeWindowMinutes;
    }

    public Task<AlertResult> EvaluateAsync(
    SensorReading reading,
    AlertContext context,
    CancellationToken cancellationToken = default)
    {
        bool isExceeded = reading.Temperature.Celsius > _temperatureThreshold;

        if (isExceeded)
        {
            // 1. 先检查时间窗口是否超时（在累加之前！）
            if (context.RecentReadings.Count > 0)
            {
                var firstExceed = context.RecentReadings
                    .OrderBy(r => r.Timestamp)
                    .FirstOrDefault(r => r.Temperature.Celsius > _temperatureThreshold)?.Timestamp;

                if (firstExceed.HasValue)
                {
                    var timeSpan = reading.Timestamp - firstExceed.Value;
                    if (timeSpan.TotalMinutes > _timeWindowMinutes)
                    {
                        // 时间窗口超时：重置计数器，把当前这次作为新窗口的第一次
                        context.ResetConsecutiveCount();
                        context.ConsecutiveExceedCount = 1;  // ← 当前这次算第 1 次
                        context.LastExceedTime = reading.Timestamp;
                        context.RecentReadings.Add(reading);

                        // 保持最新 N 条记录（避免内存膨胀）
                        if (context.RecentReadings.Count > 100)
                            context.RecentReadings.RemoveAt(0);

                        return Task.FromResult(AlertResult.NoAlert());
                    }
                }
            }

            // 2. 时间窗口有效，正常累加
            context.ConsecutiveExceedCount++;
            context.LastExceedTime = reading.Timestamp;
            context.RecentReadings.Add(reading);

            // 保持最新 N 条记录
            if (context.RecentReadings.Count > 100)
                context.RecentReadings.RemoveAt(0);

            // 3. 检查是否达到报警阈值
            if (context.ConsecutiveExceedCount >= _requiredConsecutiveCount)
            {
                var result = AlertResult.Trigger(
                    $"连续 {_requiredConsecutiveCount} 次温度超过阈值 {_temperatureThreshold}°C，当前温度 {reading.Temperature.Celsius:F1}°C",
                    AlertSeverity.Warning,
                    StrategyName,
                    _temperatureThreshold,
                    reading.Temperature.Celsius
                );

                // 报警后重置计数器（但保留最近记录，用于后续判断）
                context.ResetConsecutiveCount();
                return Task.FromResult(result);
            }
        }
        else
        {
            // 温度未超：重置连续计数
            if (context.ConsecutiveExceedCount > 0)
            {
                context.ResetConsecutiveCount();
            }
        }

        return Task.FromResult(AlertResult.NoAlert());
    }
}