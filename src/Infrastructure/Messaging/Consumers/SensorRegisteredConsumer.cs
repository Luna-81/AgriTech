// src/Infrastructure/Messaging/Consumers/SensorRegisteredConsumer.cs
using MassTransit;
using Application.Events;
using Domain.Common.Exceptions;
using Domain.Common.Interfaces;
using Domain.Farms.Entities;
using Domain.Farms.RepositoryInterfaces;
using Domain.Sensors.Entities;
using Domain.Sensors.RepositoryInterfaces;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

public class SensorRegisteredConsumer : IConsumer<SensorRegisteredEvent>
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SensorRegisteredConsumer> _logger;

    public SensorRegisteredConsumer(
        ISensorRepository sensorRepository,
        IFarmRepository farmRepository,
        IUnitOfWork unitOfWork,
        ILogger<SensorRegisteredConsumer> logger)
    {
        _sensorRepository = sensorRepository;
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SensorRegisteredEvent> context)
    {
        await Task.Delay(30000);
        var message = context.Message;
        
        _logger.LogInformation("Processing sensor registration. SensorId: {SensorId}, FarmId: {FarmId}", 
            message.SensorId, message.FarmId);

        try
        {
            // 1. 检查幂等性
            var existingSensor = await _sensorRepository.GetByIdAsync(message.SensorId);
            if (existingSensor != null)
            {
                _logger.LogWarning("Sensor already exists, skipping. SensorId: {SensorId}", message.SensorId);
                return;
            }

            // 2. 获取农场
            var farm = await _farmRepository.GetByIdAsync(message.FarmId);
            if (farm == null)
            {
                throw new DomainException($"Farm not found. FarmId: {message.FarmId}");
            }

            // 3. 创建传感器
            var temperature = Temperature.FromCelsius(message.TemperatureThreshold);
            var location = Location.FromCoordinates(message.Latitude, message.Longitude);
            var sensor = Sensor.Create(message.Name, temperature, location);
            
            // 4. ✅ 添加到农场（AddSensor 是 public）
            farm.AddSensor(sensor);

            // 5. 持久化
            await _sensorRepository.AddAsync(sensor);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Sensor persisted successfully. SensorId: {SensorId}", message.SensorId);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error processing sensor registration. SensorId: {SensorId}", message.SensorId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor registration. SensorId: {SensorId}", message.SensorId);
            throw;
        }
    }
}