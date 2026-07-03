// src/Infrastructure/Messaging/Consumers/SensorReadingConsumer.cs
using MassTransit;
using Domain.Events;
using Domain.Common.Exceptions;
using Domain.Common.Interfaces;
using Domain.Sensors.Entities;
using Domain.Sensors.RepositoryInterfaces;
using Domain.Sensors.ValueObjects;
using Microsoft.Extensions.Logging;
using Application.Events;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// 传感器读数事件消费者
/// 消费 SensorReadingRecordedEvent 消息，记录传感器读数
/// </summary>
public class SensorReadingConsumer : IConsumer<SensorReadingRecordedEvent>
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SensorReadingConsumer> _logger;

    public SensorReadingConsumer(
        ISensorRepository sensorRepository,
        IUnitOfWork unitOfWork,
        ILogger<SensorReadingConsumer> logger)
    {
        _sensorRepository = sensorRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SensorReadingRecordedEvent> context)
    {
        
        var message = context.Message;
        
        _logger.LogInformation("Processing sensor reading. SensorId: {SensorId}, Temperature: {Temperature}°C, Humidity: {Humidity}%", 
            message.SensorId, message.Temperature, message.Humidity);

        try
        {
            // 1. 获取传感器
            var sensor = await _sensorRepository.GetByIdAsync(message.SensorId);
            if (sensor == null)
            {
                _logger.LogWarning("Sensor not found for reading. SensorId: {SensorId}", message.SensorId);
                return;
            }

            // 2. 记录读数（使用 Sensor 的 AddReading 方法）
            sensor.SensorReading(message.Temperature, message.Humidity, message.Timestamp);

            // 3. 持久化
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Reading recorded successfully. SensorId: {SensorId}", message.SensorId);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error processing sensor reading. SensorId: {SensorId}", message.SensorId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor reading. SensorId: {SensorId}", message.SensorId);
            throw;
        }
    }
}