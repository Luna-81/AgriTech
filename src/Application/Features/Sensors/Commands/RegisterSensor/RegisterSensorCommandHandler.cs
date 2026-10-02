using MediatR;
using Application.Common.Models;
using Application.Common.Interfaces;      
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
    private readonly IEventPublisher _publisher;      
    private readonly ILogger<RegisterSensorCommandHandler> _logger;

    public RegisterSensorCommandHandler(
        IFarmRepository farmRepository,
        IEventPublisher publisher,                    
        ILogger<RegisterSensorCommandHandler> logger)
    {
        _farmRepository = farmRepository;
        _publisher = publisher;                      
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterSensorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. check if the farm exists
            var farm = await _farmRepository.GetByIdAsync(request.FarmId, cancellationToken);
            if (farm == null)
            {
                return Result<Guid>.Failure($"农场 '{request.FarmId}' 不存在。");
            }

            _logger.LogInformation("Farm found: {FarmId}", farm.Id);

            // 2. create the sensor aggregate
            var temperature = Temperature.FromCelsius(request.TemperatureThreshold);
            var location = Location.FromCoordinates(request.Latitude, request.Longitude);
            var sensor = Sensor.Create(request.Name, temperature, location);

            // 3. only publish the event, do not save to database here
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

            await _publisher.PublishAsync(@event, cancellationToken);   // ← 改调用

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