// src/AgriTech.Worker/Infrastructure/BatchBuffer.cs
namespace AgriTech.Worker.Infrastructure;

/// <summary>
/// 批量缓冲区 - 用于积累消息后批量处理
/// </summary>
/// <typeparam name="T">消息类型</typeparam>
public class BatchBuffer<T> : IDisposable
{
    private readonly int _batchSize;
    private readonly TimeSpan _flushInterval;
    private readonly Func<List<T>, Task> _processBatchAsync;
    private readonly ILogger _logger;
    
    private readonly List<T> _buffer = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Timer? _timer;
    private bool _isDisposed;
    private bool _isTimerRunning;

    public BatchBuffer(
        int batchSize,
        TimeSpan flushInterval,
        Func<List<T>, Task> processBatchAsync,
        ILogger logger)
    {
        _batchSize = batchSize;
        _flushInterval = flushInterval;
        _processBatchAsync = processBatchAsync;
        _logger = logger;

        // ✅ 不立即启动 Timer，等待第一条消息到达时启动
        _timer = null;
        _isTimerRunning = false;
    }

    public async Task AddAsync(T item, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _buffer.Add(item);
            
            // ✅ 达到批量大小 → 立即刷新
            if (_buffer.Count >= _batchSize)
            {
                await FlushAsync(cancellationToken);
                StopTimer();
            }
            else
            {
                // ✅ 未达到批量大小 → 启动 Timer（如果尚未启动）
                StartTimer();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void StartTimer()
    {
        if (_isTimerRunning) return;
        if (_timer == null)
        {
            _timer = new Timer(OnTimerTick, null, _flushInterval, _flushInterval);
        }
        else
        {
            _timer.Change(_flushInterval, _flushInterval);
        }
        _isTimerRunning = true;
        _logger.LogDebug("Timer started");
    }

    private void StopTimer()
    {
        if (!_isTimerRunning) return;
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _isTimerRunning = false;
        _logger.LogDebug("Timer stopped");
    }

    private async void OnTimerTick(object? state)
    {
        try
        {
            await _semaphore.WaitAsync();
            if (_buffer.Count > 0)
            {
                await FlushAsync(CancellationToken.None);
            }
            StopTimer();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch flush");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (_buffer.Count == 0) return;

        var batch = _buffer.ToList();
        _buffer.Clear();

        try
        {
            await _processBatchAsync(batch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch processing failed, re-adding {Count} items to buffer", batch.Count);
            _buffer.AddRange(batch);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        _timer?.Dispose();
        
        Task.Run(async () =>
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_buffer.Count > 0)
                {
                    await FlushAsync(CancellationToken.None);
                }
            }
            finally
            {
                _semaphore.Release();
                _semaphore.Dispose();
            }
        }).GetAwaiter().GetResult();
    }
}