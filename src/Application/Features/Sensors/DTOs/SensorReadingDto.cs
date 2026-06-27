namespace Application.Features.Sensors.DTOs;

/// <summary>
/// Sensor reading data transmission object.
/// Used only for query returns, ensuring decoupling of the API layer from the domain model.
/// </summary>

public class SensorReadingDto
{
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}