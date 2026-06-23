namespace Domain.ValueObjects;

public readonly record struct Humidity
{
    public double Moisture { get; init; }

    private Humidity(double moisture)
    {
        if (moisture < 10 || moisture > 90)
            throw new Exceptions.DomainException("Humidity not in 10 to 90");
        
        Moisture = Math.Round(moisture, 2);
    }

    public static Humidity FromMoisture(double Moisture) => new(Moisture);
}