using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Filters;

/// <summary>
/// API 异常过滤器
/// 用于捕获和记录 Controller 中未处理的同步异常
/// </summary>
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;

    /// <summary>
    /// 初始化 ApiExceptionFilterAttribute 实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 异常发生时执行
    /// </summary>
    /// <param name="context">异常上下文</param>
    public override void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception occurred.");
        base.OnException(context);
    }
}