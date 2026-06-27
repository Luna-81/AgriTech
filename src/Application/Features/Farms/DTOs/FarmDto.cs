
namespace Application.Features.Farms.DTOs;

public class FarmDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int SensorCount { get; set; }
    public int MaxCapacity { get; set; }
    public DateTime CreatedAt { get; set; }
}