using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
namespace Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) {_logger = logger;}

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var traceId = Guid.NewGuid();

        _logger.LogInformation("Executing request {RequestName} [TraceId: {TraceId}]", requestName, traceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            
            _logger.LogInformation("Request {RequestName} completed in {ElapsedMs}ms [TraceId: {TraceId}]", 
                requestName, stopwatch.ElapsedMilliseconds, traceId);
            
            return response;

        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "Request {RequestName} failed after {ElapsedMs}ms [TraceId: {TraceId}]", 
                requestName, stopwatch.ElapsedMilliseconds, traceId);

            throw;
        }
    }
}