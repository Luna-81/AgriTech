using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;

namespace WebAPI.Common;

/// <summary>
/// API 控制器基类
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Mediator 实例（延迟加载）
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// 成功响应
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>成功响应</returns>
    protected IActionResult OkResponse<T>(T data)
    {
        var traceId = HttpContext.TraceIdentifier;
        var response = ApiResponse<T>.SuccessResponse(data, traceId);
        return new OkObjectResult(response);
    }

    /// <summary>
    /// 错误响应（多条错误）
    /// </summary>
    /// <param name="errors">错误列表</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>错误响应</returns>
    protected IActionResult BadRequestResponse<T>(List<string> errors)
    {
        var traceId = HttpContext.TraceIdentifier;
        var response = ApiResponse<T>.ErrorResponse(errors, traceId);
        return new BadRequestObjectResult(response);
    }

    /// <summary>
    /// 错误响应（单条错误）
    /// </summary>
    /// <param name="error">错误消息</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>错误响应</returns>
    protected IActionResult BadRequestResponse<T>(string error)
    {
        return BadRequestResponse<T>(new List<string> { error });
    }

    /// <summary>
    /// 未找到响应
    /// </summary>
    /// <param name="error">错误消息</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>未找到响应</returns>
    protected IActionResult NotFoundResponse<T>(string error)
    {
        var traceId = HttpContext.TraceIdentifier;
        var response = ApiResponse<T>.ErrorResponse(error, traceId);
        return new NotFoundObjectResult(response);
    }

    /// <summary>
    /// 从 Result 对象转换响应
    /// </summary>
    /// <param name="result">Result 对象</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>API 响应</returns>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        var traceId = HttpContext.TraceIdentifier;
        var response = ApiResponse<T>.FromResult(result, traceId);

        if (response.Success)
            return new OkObjectResult(response);
        else
            return new BadRequestObjectResult(response);
    }

    /// <summary>
    /// 冲突响应
    /// </summary>
    /// <param name="error">错误消息</param>
    /// <typeparam name="T">数据类型</typeparam>
    /// <returns>冲突响应</returns>
    protected IActionResult ConflictResponse<T>(string error)
    {
        var traceId = HttpContext.TraceIdentifier;
        var response = ApiResponse<T>.ErrorResponse(error, traceId);
        return new ConflictObjectResult(response);
    }
}