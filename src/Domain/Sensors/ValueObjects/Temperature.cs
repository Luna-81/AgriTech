using Domain.Common.Exceptions;
namespace Domain.Sensors.ValueObjects;

public sealed record class Temperature
{
    public double Celsius {get; init;}
    private Temperature() { }

    private Temperature(double celsius)
    {
        if(celsius < -50 || celsius > 80)
            throw new DomainException("Temperature not in -50 C ~ 80 C");
        
        Celsius = Math.Round(celsius,2);
    }
    public static Temperature FromCelsius(double celsius) => new(celsius);
}