using Domain.Common.Exceptions;
namespace Domain.Sensors.ValueObjects;

public readonly record struct Humidity
{
    public double Percentage { get; init; }

    private Humidity(double percentage)
    {
        if (percentage < 10 || percentage > 90)
            throw new DomainException("Humidity not in 10 to 90");
        
        Percentage = Math.Round(percentage, 2);
    }

    public static Humidity FromPercentage(double percentage) => new(percentage);
}