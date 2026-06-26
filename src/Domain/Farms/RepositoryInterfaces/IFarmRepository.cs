using Domain.Common.Interfaces;
using Domain.Farms.Entities;
using Domain.Common.Models;

namespace Domain.Farms.RepositoryInterfaces;

/// <summary>
/// 农场仓储接口
/// </summary>
public interface IFarmRepository : IRepository<Farm>
{
    /// <summary>
    /// 根据名称获取农场
    /// </summary>
    /// <param name="name">农场名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>农场实体，如果不存在则返回 null</returns>
    Task<Farm?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查农场名称是否已存在
    /// </summary>
    /// <param name="name">农场名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取农场及其所有传感器（包含导航属性）
    /// </summary>
    /// <param name="id">农场 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含传感器的农场实体</returns>
    Task<Farm?> GetFarmWithSensorsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页获取农场列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="searchTerm">搜索关键词（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分页结果</returns>
    Task<PagedResult<Farm>> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}