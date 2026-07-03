// src/AgriTech.Worker/Worker.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgriTech.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🟢 Worker Service started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // ✅ 只检查数据库连接，不依赖 HealthCheckService
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var canConnect = await dbContext.Database.CanConnectAsync(stoppingToken);
                
                if (canConnect)
                {
                    _logger.LogInformation("✅ Worker health is healthy");
                }
                else
                {
                    _logger.LogWarning("⚠️ Database connection failed");
                }

                // 每 30 秒检查一次
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unhandled exception in worker loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("🔴 Worker Service stopped at: {time}", DateTimeOffset.Now);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Worker Service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}