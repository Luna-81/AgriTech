using Domain.Common.Interfaces;
using Domain.Sensors.Entities;
using Domain.Common.Models;

namespace Domain.Sensors.RepositoryInterfaces;

/// <summary>
/// 传感器仓储接口
/// </summary>
public interface ISensorRepository : IRepository<Sensor>
{
    /// <summary>
    /// 根据名称获取传感器
    /// </summary>
    /// <param name="name">传感器名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>传感器实体，如果不存在则返回 null</returns>
    Task<Sensor?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据农场 ID 获取所有传感器
    /// </summary>
    /// <param name="farmId">农场 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>传感器列表</returns>
    Task<List<Sensor>> GetByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有活跃的传感器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>活跃传感器列表</returns>
    Task<List<Sensor>> GetActiveSensorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取传感器及其所有读数（包含导航属性）
    /// </summary>
    /// <param name="id">传感器 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含读数的传感器实体</returns>
    Task<Sensor?> GetSensorWithReadingsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查传感器名称是否已存在
    /// </summary>
    /// <param name="name">传感器名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页获取传感器列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="farmId">农场 ID（可选）</param>
    /// <param name="searchTerm">搜索关键词（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分页结果</returns>
    Task<PagedResult<Sensor>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? farmId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取传感器的历史读数（分页）
    /// </summary>
    /// <param name="sensorId">传感器 ID</param>
    /// <param name="startDate">开始日期（可选）</param>
    /// <param name="endDate">结束日期（可选）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>读数分页结果</returns>
    Task<PagedResult<Reading>> GetReadingsPagedAsync(
        Guid sensorId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}