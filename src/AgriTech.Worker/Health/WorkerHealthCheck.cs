// src/AgriTech.Worker/Health/WorkerHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Channels;
using Domain.Sensors.Entities;

namespace AgriTech.Worker.Health;

/// <summary>
/// Worker 健康检查
/// 检查内存队列深度
/// </summary>
public class WorkerHealthCheck : IHealthCheck
{
    private readonly ILogger<WorkerHealthCheck> _logger;
    private readonly Channel<Reading>? _channel;

    public WorkerHealthCheck(
        ILogger<WorkerHealthCheck> logger,
        Channel<Reading>? channel = null)
    {
        _logger = logger;
        _channel = channel;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_channel == null)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Channel not configured, skipping"));
        }

        // 检查内存队列深度
        var queueDepth = _channel.Reader.Count;
        
        if (queueDepth > 5000)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Queue depth is {queueDepth}, exceeds limit 5000"));
        }
        else if (queueDepth > 3000)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Queue depth is {queueDepth}, approaching limit"));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Queue depth is {queueDepth}"));
        }
    }
}