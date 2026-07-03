using MediatR;
using Application.Common.Models;
using MassTransit;
using Domain.Farms.RepositoryInterfaces;
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;
using Domain.Common.Exceptions;
using Application.Events;

namespace Application.Features.Sensors.Commands.RegisterSensor;

public class RegisterSensorCommandHandler : IRequestHandler<RegisterSensorCommand, Result<Guid>>
{
    private readonly IFarmRepository _farmRepository;
    private readonly IBus _bus;
    private readonly ILogger<RegisterSensorCommandHandler> _logger;

    public RegisterSensorCommandHandler(
        IFarmRepository farmRepository,
        IBus bus,
        ILogger<RegisterSensorCommandHandler> logger)
    {
        _farmRepository = farmRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterSensorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. 验证农场是否存在
            var farm = await _farmRepository.GetByIdAsync(request.FarmId, cancellationToken);
            if (farm == null)
            {
                return Result<Guid>.Failure($"农场 '{request.FarmId}' 不存在。");
            }

            _logger.LogInformation("Farm found: {FarmId}", farm.Id);

            // 2. 创建传感器实体
            var temperature = Temperature.FromCelsius(request.TemperatureThreshold);
            var location = Location.FromCoordinates(request.Latitude, request.Longitude);
            var sensor = Sensor.Create(request.Name, temperature, location);

            // 3. ✅ 只发布消息，不保存数据库
            var @event = new SensorRegisteredEvent
            {
                SensorId = sensor.Id,
                Name = sensor.Name,
                TemperatureThreshold = sensor.TemperatureThreshold.Celsius,
                Latitude = sensor.Location.Latitude,
                Longitude = sensor.Location.Longitude,
                FarmId = request.FarmId,
                RegisteredAt = DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to publish event for SensorId: {SensorId}", sensor.Id);
            
            await _bus.Publish(@event, cancellationToken);
            
            _logger.LogInformation("Event published successfully for SensorId: {SensorId}", sensor.Id);

            return Result<Guid>.Success(sensor.Id);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error while registering sensor");
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while registering sensor: {Message}", ex.Message);
            return Result<Guid>.Failure($"注册传感器时发生错误：{ex.Message}");
        }
    }
}