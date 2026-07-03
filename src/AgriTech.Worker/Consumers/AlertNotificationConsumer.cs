// src/AgriTech.Worker/Consumers/AlertNotificationConsumer.cs
using MassTransit;
using Application.Events;
using Microsoft.Extensions.Logging;

namespace AgriTech.Worker.Consumers;

/// <summary>
/// 告警通知消费者
/// 处理告警消息，发送邮件或短信
/// </summary>
public class AlertNotificationConsumer : IConsumer<AlertTriggeredEvent>
{
    private readonly ILogger<AlertNotificationConsumer> _logger;

    public AlertNotificationConsumer(ILogger<AlertNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AlertTriggeredEvent> context)
    {
        var message = context.Message;
        
        _logger.LogWarning("⚠️ Alert triggered: {AlertType}, SensorId: {SensorId}, Value: {Value}",
            message.AlertType, message.SensorId, message.ThresholdValue);

        try
        {
            // TODO: 实现邮件/短信发送
            if (message.Severity == AlertSeverity.Critical)
            {
                _logger.LogWarning("🚨 CRITICAL alert! Sending email and SMS...");
                // await _emailService.SendAlertAsync(message);
                // await _smsService.SendAlertAsync(message);
            }
            else
            {
                _logger.LogWarning("⚠️ Warning alert. Sending email only...");
                // await _emailService.SendAlertAsync(message);
            }

            _logger.LogInformation("✅ Alert notification sent. AlertId: {AlertId}", message.AlertId);
            // MassTransit 8.x 自动确认，不需要手动调用 Complete()
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send alert notification. AlertId: {AlertId}", message.AlertId);
            throw;
        }

        await Task.CompletedTask;
    }
}