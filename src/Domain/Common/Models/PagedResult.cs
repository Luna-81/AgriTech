namespace Domain.Common.Models;

/// <summary>
/// 分页结果
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPreviousPage => Page > 1;
    
    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PagedResult() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="items">数据列表</param>
    /// <param name="totalCount">总记录数</param>
    /// <param name="page">当前页码</param>
    /// <param name="pageSize">每页大小</param>
    public PagedResult(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// 创建分页结果
    /// </summary>
    public static PagedResult<T> Create(List<T> items, int totalCount, int page, int pageSize)
    {
        return new PagedResult<T>(items, totalCount, page, pageSize);
    }
}