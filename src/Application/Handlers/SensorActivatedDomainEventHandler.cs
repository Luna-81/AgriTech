// src/Application/Handlers/SensorActivatedDomainEventHandler.cs
using MediatR;
using MassTransit;
using Application.Events;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Application.Handlers;

/// <summary>
/// 传感器激活领域事件处理器
/// 将领域事件转换为集成事件发送到 RabbitMQ
/// </summary>
public class SensorActivatedDomainEventHandler : INotificationHandler<SensorActivatedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<SensorActivatedDomainEventHandler> _logger;

    public SensorActivatedDomainEventHandler(
        IBus bus,
        ILogger<SensorActivatedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(SensorActivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // 将领域事件转换为集成事件（Integration Event）发送到 RabbitMQ
        var integrationEvent = new SensorActivatedIntegrationEvent
        {
            SensorId = notification.SensorId,
            ActivatedAt = notification.ActivatedAt
        };

        await _bus.Publish(integrationEvent, cancellationToken);
        
        _logger.LogInformation("SensorActivatedIntegrationEvent published. SensorId: {SensorId}", 
            notification.SensorId);
    }
}