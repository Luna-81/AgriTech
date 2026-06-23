using Domain.ValueObjects;
using Domain.Exceptions;
using Domain.Enums;

namespace Domain.Entities;

public class Sensor
{
    public Guid Id {get; private init;}
    public string Name { get; private set; }
    public Temperature TemperatureThreshold{get; private set;}
    public Location Location { get; private init; }  
    public SensorStatus Status { get; private set; }
    public DateTime InstallationDate { get; private init; }

    private Sensor(Guid id, string name, Temperature threshold, Location location)
    {
        Id = id;
        Name = name;
        TemperatureThreshold = threshold;
        Location = location;
        Status = SensorStatus.Inactive; 
        InstallationDate = DateTime.UtcNow;
    }

    public static Sensor Create(string name, Temperature threshold, Location location)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new DomainException("sensor name can not null");
        
        return new Sensor(Guid.NewGuid(), name.Trim(), threshold,location);
    }

    // status
    public void Activate()
    {
        if(Status == SensorStatus.Active)
            throw new DomainException("Senseor status at Active");

        if(Status == SensorStatus.Maintenance)
            throw new DomainException("Senseor status at Maintenance");

        Status = SensorStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == SensorStatus.Inactive)
            throw new DomainException("Senseor status at Deactivate");

        Status = SensorStatus.Inactive;
    }

    public void PutUnderMaintenance()
    {
        if (Status == SensorStatus.Maintenance)
            throw new DomainException("Senseor status at Maintenance");

        Status = SensorStatus.Maintenance;
    }

    // when not in Maintenance, can upate value
    public void UpdateThreshold(Temperature newThreshold)
    {
        if (Status == SensorStatus.Maintenance)
            throw new DomainException("when Maintenance, can't update values ");

        TemperatureThreshold = newThreshold;
    }

}