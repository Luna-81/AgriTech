using MediatR;
using Application.Common.Models;

namespace Application.Features.Sensors.Commands.RecordSensorReading;

/// <summary>
/// 记录传感器读数命令
/// </summary>
public class RecordSensorReadingCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// 传感器 ID
    /// </summary>
    public Guid SensorId { get; set; }
    
    /// <summary>
    /// 温度值（摄氏度）
    /// </summary>
    public double Temperature { get; set; }
    
    /// <summary>
    /// 湿度值（百分比）
    /// </summary>
    public double Humidity { get; set; }
    
    /// <summary>
    /// 读数时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
}