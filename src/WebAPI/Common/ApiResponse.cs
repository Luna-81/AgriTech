using System.Diagnostics;
using Application.Common.Models;

namespace WebAPI.Common;

/// <summary>
/// 统一 API 响应模型
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 初始化 ApiResponse 实例
    /// </summary>
    public ApiResponse()
    {
        Errors = new List<string>();
        TraceId = string.Empty;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 错误列表
    /// </summary>
    public List<string> Errors { get; set; }

    /// <summary>
    /// 时间戳（UTC）
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 追踪 ID（用于链路追踪）
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="traceId">追踪 ID</param>
    /// <returns>成功响应</returns>
    public static ApiResponse<T> SuccessResponse(T data, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Errors = new List<string>(),
            Timestamp = DateTime.UtcNow,
            TraceId = traceId ?? Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    /// 创建错误响应（单条错误）
    /// </summary>
    /// <param name="error">错误消息</param>
    /// <param name="traceId">追踪 ID</param>
    /// <returns>错误响应</returns>
    public static ApiResponse<T> ErrorResponse(string error, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Errors = new List<string> { error },
            Timestamp = DateTime.UtcNow,
            TraceId = traceId ?? Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    /// 创建错误响应（多条错误）
    /// </summary>
    /// <param name="errors">错误列表</param>
    /// <param name="traceId">追踪 ID</param>
    /// <returns>错误响应</returns>
    public static ApiResponse<T> ErrorResponse(List<string> errors, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Errors = errors,
            Timestamp = DateTime.UtcNow,
            TraceId = traceId ?? Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    /// 从 Result 对象转换
    /// </summary>
    /// <param name="result">Result 对象</param>
    /// <param name="traceId">追踪 ID</param>
    /// <returns>API 响应</returns>
    public static ApiResponse<T> FromResult(Result<T> result, string? traceId = null)
    {
        if (result.IsSuccess)
            return SuccessResponse(result.Value!, traceId);
        else
            return ErrorResponse(result.Errors, traceId);
    }
}