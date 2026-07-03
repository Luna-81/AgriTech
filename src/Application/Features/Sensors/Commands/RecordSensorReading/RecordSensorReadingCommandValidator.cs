using FluentValidation;

namespace Application.Features.Sensors.Commands.RecordSensorReading;

/// <summary>
/// 记录传感器读数命令验证器
/// </summary>
public class RecordSensorReadingCommandValidator : AbstractValidator<RecordSensorReadingCommand>
{
    public RecordSensorReadingCommandValidator()
    {
        RuleFor(x => x.SensorId)
            .NotEmpty().WithMessage("传感器 ID 不能为空。");

        RuleFor(x => x.Temperature)
            .GreaterThan(-273.15).WithMessage("温度不能低于绝对零度（-273.15°C）。")
            .LessThan(1000).WithMessage("温度不能超过 1000°C。");

        RuleFor(x => x.Humidity)
            .InclusiveBetween(0, 100).WithMessage("湿度必须在 0% 到 100% 之间。");

        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("时间戳不能为空。")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("时间戳不能晚于当前时间。");
    }
}