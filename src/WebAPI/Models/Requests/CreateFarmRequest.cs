namespace WebAPI.Models.Requests;

/// <summary>
/// 创建农场请求
/// </summary>
public class CreateFarmRequest
{
    /// <summary>
    /// 农场名称
    /// </summary>
    /// <example>智慧农场 1号</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 纬度
    /// </summary>
    /// <example>31.2304</example>
    public double Latitude { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    /// <example>121.4737</example>
    public double Longitude { get; set; }

    /// <summary>
    /// 最大传感器容量
    /// </summary>
    /// <example>50</example>
    public int? MaxCapacity { get; set; }
}