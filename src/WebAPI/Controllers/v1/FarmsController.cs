using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Farms.Commands.CreateFarm;
using Application.Features.Farms.Queries.GetFarms;
using Application.Features.Farms.Queries.GetFarmById;
using Application.Features.Farms.DTOs;
using Domain.Common.Models;
using WebAPI.Common;
using WebAPI.Models.Requests;

namespace WebAPI.Controllers.v1;

/// <summary>
/// 农场管理 API
/// 提供农场的创建、查询等功能
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class FarmsController : ApiControllerBase
{
    private readonly ILogger<FarmsController> _logger;

    /// <summary>
    /// 初始化 FarmsController 实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public FarmsController(ILogger<FarmsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 创建农场
    /// </summary>
    /// <param name="request">农场创建请求</param>
    /// <returns>创建的农场 ID</returns>
    /// <response code="200">创建成功</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFarm([FromBody] CreateFarmRequest request)
    {
        var command = new CreateFarmCommand
        {
            Name = request.Name,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            MaxCapacity = request.MaxCapacity ?? 50
        };

        var result = await Mediator.Send(command);
        return FromResult(result);
    }

    /// <summary>
    /// 获取所有农场（分页）
    /// </summary>
    /// <param name="page">页码（默认 1）</param>
    /// <param name="pageSize">每页大小（默认 20，最大 100）</param>
    /// <param name="searchTerm">搜索关键词（可选）</param>
    /// <returns>农场列表</returns>
    /// <response code="200">返回农场列表</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FarmDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFarms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = new GetFarmsQuery 
        { 
            Page = page, 
            PageSize = pageSize,
            SearchTerm = searchTerm
        };
        
        var result = await Mediator.Send(query);
        return FromResult(result);
    }
}