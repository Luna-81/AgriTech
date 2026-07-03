// src/AgriTech.Worker/Consumers/SensorReadingBatchConsumer.cs
using MassTransit;
using Application.Events;
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AgriTech.Worker.Consumers;

public class SensorReadingBatchConsumer : IConsumer<SensorReadingRecordedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorReadingBatchConsumer> _logger;
    
    // ✅ 使用 ConcurrentBag 存储消息
    private static readonly ConcurrentBag<SensorReadingRecordedEvent> _buffer = new();
    private static int _messageCount = 0;
    private static readonly object _lock = new();
    private static Timer? _timer;
    private static bool _isProcessing = false;
    
    private const int BatchSize = 100;
    private const int FlushIntervalSeconds = 5;

    public SensorReadingBatchConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<SensorReadingBatchConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        
        // ✅ 启动定时器
        if (_timer == null)
        {
            _timer = new Timer(FlushTimer, null, 
                TimeSpan.FromSeconds(FlushIntervalSeconds), 
                TimeSpan.FromSeconds(FlushIntervalSeconds));
            _logger.LogInformation("🔄 批量消费者定时器已启动，间隔 {FlushIntervalSeconds} 秒", FlushIntervalSeconds);
        }
    }

    public async Task Consume(ConsumeContext<SensorReadingRecordedEvent> context)
    {
        // ✅ 添加消息到缓冲区
        lock (_lock)
        {
            _buffer.Add(context.Message);
            _messageCount++;
            
            // 每 10 条记录一次日志
            if (_messageCount % 10 == 0 || _messageCount == 1)
            {
                _logger.LogInformation($"📦 缓冲区: {_messageCount} 条消息");
            }
            
            // ✅ 达到批量大小 → 立即处理
            if (_messageCount >= BatchSize)
            {
                _logger.LogInformation($"🎯 达到批量阈值 {BatchSize}，立即处理");
                var messages = _buffer.ToList();
                _buffer.Clear();
                _messageCount = 0;
                
                // 异步处理
                _ = Task.Run(async () => await ProcessBatchAsync(messages));
            }
        }
    }

    private async void FlushTimer(object? state)
    {
        // ✅ 防止重复处理
        if (_isProcessing) return;
        
        try
        {
            lock (_lock)
            {
                if (_messageCount > 0)
                {
                    _logger.LogInformation($"⏰ 定时器触发，处理 {_messageCount} 条消息");
                    var messages = _buffer.ToList();
                    _buffer.Clear();
                    var count = _messageCount;
                    _messageCount = 0;
                    
                    _isProcessing = true;
                    _ = Task.Run(async () => 
                    {
                        try
                        {
                            await ProcessBatchAsync(messages);
                        }
                        finally
                        {
                            _isProcessing = false;
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "定时器处理失败");
            _isProcessing = false;
        }
    }

    private async Task ProcessBatchAsync(List<SensorReadingRecordedEvent> messages)
    {
        if (messages.Count == 0) return;
        
        _logger.LogInformation($"🔄 处理批量: {messages.Count} 条读数");

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // ✅ 按 SensorId 分组
            var grouped = messages.GroupBy(m => m.SensorId);

            foreach (var group in grouped)
            {
                var sensorId = group.Key;
                var readings = group.ToList();

                var sensor = await dbContext.Sensors
                    .FirstOrDefaultAsync(s => s.Id == sensorId);

                if (sensor == null)
                {
                    _logger.LogWarning($"⚠️ 传感器不存在: {sensorId}");
                    continue;
                }

                foreach (var msg in readings)
                {
                    sensor.SensorReading(msg.Temperature, msg.Humidity, msg.Timestamp);
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation($"✅ 批量处理完成: {messages.Count} 条读数");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ 批量处理失败: {messages.Count} 条读数");
            throw;
        }
    }
}