using MediatR;
using Application.Common.Models;
using Application.Events;
using Application.Common.Interfaces;      
using Domain.Sensors.RepositoryInterfaces;
using Domain.Sensors.ValueObjects;
using Domain.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Domain.Common.Exceptions;


namespace Application.Features.Sensors.Commands.RecordSensorReading;

/// <summary>
/// record sensor reading command handler that handles the RecordSensorReadingCommand.
/// </summary>
public class RecordSensorReadingCommandHandler : IRequestHandler<RecordSensorReadingCommand, Result<bool>>
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _publisher;     
    private readonly ILogger<RecordSensorReadingCommandHandler> _logger;

    public RecordSensorReadingCommandHandler(
        ISensorRepository sensorRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher publisher,                    
        ILogger<RecordSensorReadingCommandHandler> logger)
    {
        _sensorRepository = sensorRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;                      
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RecordSensorReadingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. get the sensor from the repository
            var sensor = await _sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
            if (sensor == null)
            {
                return Result<bool>.Failure($"传感器 '{request.SensorId}' 不存在。");
            }

            // 2. record the reading in the sensor aggregate
            sensor.SensorReading(request.Temperature, request.Humidity, request.Timestamp);

            // 3. save to database
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reading recorded. SensorId: {SensorId}, Temperature: {Temperature}°C, Humidity: {Humidity}%", 
                request.SensorId, request.Temperature, request.Humidity);

            // 4. publish event to message queue
            var @event = new SensorReadingRecordedEvent
            {
                SensorId = request.SensorId,
                Temperature = request.Temperature,
                Humidity = request.Humidity,
                Timestamp = request.Timestamp
            };
            await _publisher.PublishAsync(@event, cancellationToken);  
            _logger.LogInformation("Reading event published. SensorId: {SensorId}", request.SensorId);

            return Result<bool>.Success(true);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error while recording reading. SensorId: {SensorId}", request.SensorId);
            return Result<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while recording reading. SensorId: {SensorId}", request.SensorId);
            return Result<bool>.Failure($"Error occurred while recording reading: {ex.Message}");
        }
    }
}