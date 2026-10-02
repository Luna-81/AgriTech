// src/Application/Handlers/SensorActivatedNotificationHandler.cs
using MediatR;
using Application.Common.Interfaces;
using Application.Events;
using Microsoft.Extensions.Logging;

namespace Application.Handlers;

/// <summary>
/// notification handler for SensorActivatedNotification.
/// handles the SensorActivatedNotification and publishes the corresponding integration event to the event bus.
/// </summary>
public class SensorActivatedNotificationHandler : INotificationHandler<SensorActivatedNotification>
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<SensorActivatedNotificationHandler> _logger;

    public SensorActivatedNotificationHandler(
        IEventPublisher publisher,
        ILogger<SensorActivatedNotificationHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(SensorActivatedNotification notification, CancellationToken cancellationToken)
    {
        // 将通知转换为集成事件（Integration Event）发送到消息队列
        var integrationEvent = new SensorActivatedIntegrationEvent
        {
            SensorId = notification.SensorId,
            ActivatedAt = notification.ActivatedAt
        };

        await _publisher.PublishAsync(integrationEvent, cancellationToken);

        _logger.LogInformation("SensorActivatedIntegrationEvent published. SensorId: {SensorId}",
            notification.SensorId);
    }
}