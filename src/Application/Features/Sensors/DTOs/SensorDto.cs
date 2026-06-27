using Domain.Sensors.Enums;

namespace Application.Features.Sensors.DTOs;

public class SensorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double TemperatureThreshold { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public SensorStatus Status { get; set; }
    public DateTime InstallationDate { get; set; }
    public int ReadingCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}