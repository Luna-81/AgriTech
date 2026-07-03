using Application.Features.Sensors.Commands.RegisterSensor;
using Application.Features.Sensors.Commands.RecordSensorReading;
using Application.Features.Sensors.Queries.GetSensorHistory;
using Application.Features.Sensors.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using WebAPI.Models.Requests;

namespace WebAPI.Controllers.v1;

/// <summary>
/// 传感器管理 API
/// 提供传感器的注册、查询、激活等功能
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SensorsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// 初始化 SensorsController 实例
    /// </summary>
    /// <param name="sender">MediatR 发送器</param>
    public SensorsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// 注册新传感器
    /// </summary>
    /// <param name="command">注册传感器命令，包含传感器信息</param>
    /// <returns>注册成功的传感器 ID</returns>
    /// <response code="200">注册成功</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterSensorCommand command)
    {
        var result = await _sender.Send(command);

        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(result.Errors);
    }

    /// <summary>
    /// 查询传感器历史数据
    /// </summary>
    /// <param name="id">传感器 ID</param>
    /// <param name="page">页码（默认 1）</param>
    /// <param name="pageSize">每页大小（默认 20，最大 100）</param>
    /// <returns>传感器历史数据列表</returns>
    /// <response code="200">返回历史数据</response>
    /// <response code="400">请求参数无效</response>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(List<SensorReadingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
        Guid id, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = new GetSensorHistoryQuery 
        { 
            SensorId = id, 
            Page = page, 
            PageSize = pageSize 
        };
        
        var result = await _sender.Send(query);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(result.Errors);
    }

    /// <summary>
    /// 记录传感器读数
    /// </summary>
    /// <param name="id">传感器 ID（从路由获取）</param>
    /// <param name="request">读数请求</param>
    /// <returns>操作结果</returns>
    /// <response code="200">记录成功</response>
    /// <response code="404">传感器不存在</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost("{id}/readings")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordReading(
        [FromRoute] Guid id,  // ✅ 添加 [FromRoute]
        [FromBody] RecordReadingRequest request)
    {
        var command = new RecordSensorReadingCommand
        {
            SensorId = id,
            Temperature = request.Temperature,
            Humidity = request.Humidity,
            Timestamp = DateTime.UtcNow
        };
        var result = await _sender.Send(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Errors);
    }
}