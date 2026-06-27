using Domain.Sensors.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Converters;

public class SensorStatusConverter : ValueConverter<SensorStatus, string>
{
    public SensorStatusConverter()
        : base(
            v => v.ToString(),
            v => (SensorStatus)Enum.Parse(typeof(SensorStatus), v)
        )
    {
    }
}