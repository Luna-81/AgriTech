

namespace  Domain.Common.Exceptions;

/// <summary>
/// 资源未找到异常
/// 当请求的资源不存在时抛出
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// 使用实体名称和 ID 创建异常
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="id">实体 ID</param>
    public NotFoundException(string entityName, Guid id) 
        : base($"{entityName} with ID '{id}' was not found.")
    {
    }

    /// <summary>
    /// 使用实体名称和字符串标识符创建异常
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="identifier">标识符</param>
    public NotFoundException(string entityName, string identifier) 
        : base($"{entityName} with identifier '{identifier}' was not found.")
    {
    }

    /// <summary>
    /// 使用自定义消息创建异常
    /// </summary>
    /// <param name="message">错误消息</param>
    public NotFoundException(string message) : base(message)
    {
    }
}