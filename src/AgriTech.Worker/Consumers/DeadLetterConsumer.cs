// src/AgriTech.Worker/Consumers/DeadLetterConsumer.cs
using MassTransit;
using Application.Events;
using Microsoft.Extensions.Logging;

namespace AgriTech.Worker.Consumers;

/// <summary>
/// 死信消费者
/// 处理无法正常消费的消息
/// </summary>
public class DeadLetterConsumer : IConsumer<DeadLetterMessage>
{
    private readonly ILogger<DeadLetterConsumer> _logger;

    public DeadLetterConsumer(ILogger<DeadLetterConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeadLetterMessage> context)
    {
        var message = context.Message;
        
        _logger.LogError("💀 Dead letter message received: {MessageType}, Reason: {Reason}, Exception: {Exception}",
            message.OriginalMessageType,
            message.Reason,
            message.Exception);

        try
        {
            // TODO: 记录死信到数据库
            // await _deadLetterService.RecordDeadLetterAsync(message);
            
            // TODO: 发送告警通知管理员
            // await _alertService.NotifyAdminAsync(message);

            _logger.LogInformation("✅ Dead letter processed. MessageType: {MessageType}", message.OriginalMessageType);
            // MassTransit 8.x 自动确认
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process dead letter. MessageType: {MessageType}", message.OriginalMessageType);
            throw;
        }
        await Task.CompletedTask;
    }
}