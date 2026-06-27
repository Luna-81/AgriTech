using Domain.Sensors.ValueObjects;
using Domain.Common.Exceptions;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class TemperatureTests
{
    [Fact]
    public void Create_Invalid_temperature_Should_Throw_DomainException()
    {
        Assert.Throws<DomainException>(()=>Temperature.FromCelsius(200));
    }
}