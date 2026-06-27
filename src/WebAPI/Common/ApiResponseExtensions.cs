using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Common;

/// <summary>
/// ApiResponse 扩展方法
/// 提供将 ApiResponse 转换为 IActionResult 的扩展方法
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// 将 ApiResponse 转换为 IActionResult
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="response">API 响应对象</param>
    /// <returns>IActionResult 结果</returns>
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response)
    {
        if (response.Success)
            return new OkObjectResult(response);
        else
            return new BadRequestObjectResult(response);
    }

    /// <summary>
    /// 将 ApiResponse 转换为 OkObjectResult
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="response">API 响应对象</param>
    /// <returns>OkObjectResult</returns>
    public static OkObjectResult ToOkResult<T>(this ApiResponse<T> response)
    {
        return new OkObjectResult(response);
    }

    /// <summary>
    /// 将 ApiResponse 转换为 BadRequestObjectResult
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="response">API 响应对象</param>
    /// <returns>BadRequestObjectResult</returns>
    public static BadRequestObjectResult ToBadRequestResult<T>(this ApiResponse<T> response)
    {
        return new BadRequestObjectResult(response);
    }

    /// <summary>
    /// 将 ApiResponse 转换为 NotFoundObjectResult
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="response">API 响应对象</param>
    /// <returns>NotFoundObjectResult</returns>
    public static NotFoundObjectResult ToNotFoundResult<T>(this ApiResponse<T> response)
    {
        return new NotFoundObjectResult(response);
    }

    /// <summary>
    /// 将 ApiResponse 转换为 ConflictObjectResult
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    /// <param name="response">API 响应对象</param>
    /// <returns>ConflictObjectResult</returns>
    public static ConflictObjectResult ToConflictResult<T>(this ApiResponse<T> response)
    {
        return new ConflictObjectResult(response);
    }
}