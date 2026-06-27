using Domain.Sensors.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Converters;

public class HumidityConverter : ValueConverter<Humidity, double>
{
    public HumidityConverter()
        : base(
            v => v.Percentage,
            v => Humidity.FromPercentage(v)
        )
    {
    }
}