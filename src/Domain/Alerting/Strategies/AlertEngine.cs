// src/Domain/Alerting/AlertEngine.cs
using Domain.Sensors.Entities;

namespace Domain.Alerting;

public class AlertEngine
{
    private readonly List<IAlertStrategy> _strategies;
    private readonly Dictionary<Guid, AlertContext> _contexts = new();

    public AlertEngine(IEnumerable<IAlertStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public async Task<List<AlertResult>> EvaluateReadingsAsync(
        Sensor sensor,
        List<SensorReading> readings,
        CancellationToken cancellationToken = default)
    {
        var results = new List<AlertResult>();

        if (!_contexts.TryGetValue(sensor.Id, out var context))
        {
            context = new AlertContext
            {
                SensorId = sensor.Id,
                SensorStatus = sensor.Status
            };
            _contexts[sensor.Id] = context;
        }

        context.RecentReadings.AddRange(readings);
        context.SensorStatus = sensor.Status;

        if (context.RecentReadings.Count > 100)
        {
            context.RecentReadings = context.RecentReadings
                .OrderByDescending(r => r.Timestamp)
                .Take(100)
                .ToList();
        }

        foreach (var reading in readings)
        {
            foreach (var strategy in _strategies)
            {
                try
                {
                    var result = await strategy.EvaluateAsync(reading, context, cancellationToken);
                    if (result.IsAlertTriggered)
                    {
                        results.Add(result);
                    }
                }
                catch
                {
                    // 静默处理异常
                }
            }
        }

        return results;
    }

    public void RemoveContext(Guid sensorId)
    {
        _contexts.Remove(sensorId);
    }
}