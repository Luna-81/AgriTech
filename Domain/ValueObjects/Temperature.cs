namespace Domain.ValueObjects;

public readonly record struct Temperature
{
    public double Celsius {get; init;}

    private Temperature(double celsius)
    {
        if(celsius < -50 || celsius > 80)
            throw new Exceptions.DomainException("Temperature not in -50 C ~ 80 C");
        Celsius = Math.Round(celsius,2);
    }
    public static Temperature FromCelsius(double celsius) => new(celsius);
}