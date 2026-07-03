// src/WebAPI/Models/Requests/RecordReadingRequest.cs
namespace WebAPI.Models.Requests;

/// <summary>
/// 记录传感器读数请求
/// </summary>
public class RecordReadingRequest
{
    /// <summary>
    /// 温度值（摄氏度）
    /// </summary>
    /// <example>25.5</example>
    public double Temperature { get; set; }

    /// <summary>
    /// 湿度值（百分比）
    /// </summary>
    /// <example>60.0</example>
    public double Humidity { get; set; }
}