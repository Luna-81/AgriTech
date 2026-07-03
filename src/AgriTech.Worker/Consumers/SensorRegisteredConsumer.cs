// src/AgriTech.Worker/Consumers/SensorRegisteredConsumer.cs
using MassTransit;
using Application.Events;
using Domain.Common.Exceptions;
using Domain.Farms.Entities;
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgriTech.Worker.Consumers;

public class SensorRegisteredConsumer : IConsumer<SensorRegisteredEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorRegisteredConsumer> _logger;

    public SensorRegisteredConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<SensorRegisteredConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SensorRegisteredEvent> context)
    {
        //await Task.Delay(60000);
        var message = context.Message;

        _logger.LogInformation("🔄 Worker processing sensor registration. SensorId: {SensorId}, FarmId: {FarmId}",
            message.SensorId, message.FarmId);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // 1. 检查幂等性
            var exists = await dbContext.Sensors.AnyAsync(s => s.Id == message.SensorId);
            if (exists)
            {
                _logger.LogWarning("⚠️ Sensor already exists, skipping. SensorId: {SensorId}", message.SensorId);
                return;
            }

            // 2. 验证农场是否存在
            var farm = await dbContext.Farms.FirstOrDefaultAsync(f => f.Id == message.FarmId);
            if (farm == null)
            {
                throw new DomainException($"Farm not found: {message.FarmId}");
            }

            // 3. 创建传感器
            var temperature = Temperature.FromCelsius(message.TemperatureThreshold);
            var location = Location.FromCoordinates(message.Latitude, message.Longitude);
            var sensor = Sensor.Create(message.Name, temperature, location);

            // 4. ✅ 直接保存 Sensor（不更新 Farm）
            await dbContext.Sensors.AddAsync(sensor);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("✅ Sensor persisted successfully. SensorId: {SensorId}", message.SensorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing sensor registration. SensorId: {SensorId}", message.SensorId);
            throw;
        }
    }
}