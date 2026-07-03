using MediatR;
using Application.Common.Models;
using Application.Events;
using MassTransit;
using Domain.Sensors.RepositoryInterfaces;
using Domain.Sensors.ValueObjects;
using Domain.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Domain.Common.Exceptions;

namespace Application.Features.Sensors.Commands.RecordSensorReading;

/// <summary>
/// 记录传感器读数命令处理器
/// </summary>
public class RecordSensorReadingCommandHandler : IRequestHandler<RecordSensorReadingCommand, Result<bool>>
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBus _bus;
    private readonly ILogger<RecordSensorReadingCommandHandler> _logger;

    public RecordSensorReadingCommandHandler(
        ISensorRepository sensorRepository,
        IUnitOfWork unitOfWork,
        IBus bus,
        ILogger<RecordSensorReadingCommandHandler> logger)
    {
        _sensorRepository = sensorRepository;
        _unitOfWork = unitOfWork;
        _bus = bus;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RecordSensorReadingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. 获取传感器
            var sensor = await _sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
            if (sensor == null)
            {
                return Result<bool>.Failure($"传感器 '{request.SensorId}' 不存在。");
            }

            // 2. 记录读数（Domain 层验证）
            sensor.SensorReading(request.Temperature, request.Humidity, request.Timestamp);

            // 3. 保存到数据库
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reading recorded. SensorId: {SensorId}, Temperature: {Temperature}°C, Humidity: {Humidity}%", 
                request.SensorId, request.Temperature, request.Humidity);

            // 4. ✅ 发布消息到 RabbitMQ
            var @event = new SensorReadingRecordedEvent
            {
                SensorId = request.SensorId,
                Temperature = request.Temperature,
                Humidity = request.Humidity,
                Timestamp = request.Timestamp
            };
            await _bus.Publish(@event, cancellationToken);
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
            return Result<bool>.Failure("记录读数时发生错误，请稍后重试。");
        }
    }
}