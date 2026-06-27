using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using FluentValidation;
using Domain.Common.Exceptions;
using WebAPI.Common;

namespace WebAPI.Exceptions;

/// <summary>
/// 全局异常处理器
/// 实现 IExceptionHandler 接口，统一处理所有未捕获的异常
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// 初始化 GlobalExceptionHandler 实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="environment">Web 主机环境</param>
    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="httpContext">HTTP 上下文</param>
    /// <param name="exception">异常对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否处理成功</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. 获取 TraceId
        var traceId = httpContext.TraceIdentifier;
        
        // 2. 记录日志（包含 TraceId）
        _logger.LogError(exception, 
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId, httpContext.Request.Path, httpContext.Request.Method);

        // 3. 根据异常类型决定 HTTP 状态码和错误消息
        var (statusCode, errors) = exception switch
        {
            // 资源未找到 → 404（放在最前面优先匹配）
            NotFoundException notFoundEx =>
                (StatusCodes.Status404NotFound, new List<string> { notFoundEx.Message }),
            
            // 领域异常 → 400
            DomainException domainEx => 
                (StatusCodes.Status400BadRequest, new List<string> { domainEx.Message }),
            
            // FluentValidation 验证异常 → 400
            ValidationException validationEx =>
                (StatusCodes.Status400BadRequest, validationEx.Errors.Select(e => e.ErrorMessage).ToList()),
            
            // 其他未预期异常 → 500（默认）
            _ => 
                (StatusCodes.Status500InternalServerError, new List<string> { "服务器内部错误，请稍后重试。" })
        };

        // 4. 特殊处理数据库异常（单独处理，因为需要检查 InnerException）
        // 注意：这里不能放在 switch 表达式中，因为 DbUpdateException 可能被上面的 _ 捕获
        if (exception is DbUpdateException dbEx && dbEx.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505") // 唯一约束冲突
            {
                statusCode = StatusCodes.Status409Conflict;
                errors = new List<string> { "数据已存在，请检查唯一字段。", pgEx.Detail ?? pgEx.Message };
            }
            else if (pgEx.SqlState == "23503") // 外键约束冲突
            {
                statusCode = StatusCodes.Status400BadRequest;
                errors = new List<string> { "引用的数据不存在。", pgEx.Detail ?? pgEx.Message };
            }
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                errors = new List<string> { "数据库操作失败，请稍后重试。" };
            }
        }

        // 5. 开发环境返回详细错误（包含堆栈信息）
        if (_environment.IsDevelopment() && statusCode == StatusCodes.Status500InternalServerError)
        {
            errors = new List<string> 
            { 
                exception.Message,
                exception.StackTrace ?? "No stack trace available"
            };
        }

        // 6. 构建统一响应
        var response = new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = errors,
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };

        // 7. 返回 Problem Details (RFC 7807)
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}