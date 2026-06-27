using Domain.Sensors.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Converters;

public class TemperatureConverter : ValueConverter<Temperature, double>
{
    public TemperatureConverter() 
        : base(
            v => v.Celsius, 
            v => Temperature.FromCelsius(v)
        ) 
    {
    }
}