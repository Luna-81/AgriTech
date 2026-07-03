using Domain.Sensors.ValueObjects;
using Domain.Common.Exceptions;
using Domain.Sensors.Enums;
using Domain.Common.Interfaces;
using Domain.Shared.ValueObjects;
using Domain.Events;

namespace Domain.Sensors.Entities;

public record Reading(DateTime Timestamp, double Temperature, double Humidity);
public class Sensor : IAggregateRoot, IAuditableEntity
{
    public Guid Id {get; private init;}
    public string Name { get; private set; } = null!;
    public Temperature TemperatureThreshold{get; private set;} = null!;
    public Location Location { get; private init; }   = null!;
    public SensorStatus Status { get; private set; }
    public DateTime InstallationDate { get; private init; }
    private readonly List<Reading> _readings = new();
    public IReadOnlyCollection<Reading> Readings => _readings.AsReadOnly();

    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
   
    private Sensor() { }

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

    public void SensorReading(double temp, double humidity)
    {
        if (Status != SensorStatus.Active)
            throw new DomainException("Only Active sensors can receive readings.");

        var reading = new Reading(DateTime.UtcNow, temp, humidity);
        _readings.Add(reading);
    }

    public void SensorReading(double temp, double humidity, DateTime timestamp)
    {
        if (Status != SensorStatus.Active)
            throw new DomainException("Only Active sensors can receive readings.");

        var reading = new Reading(timestamp, temp, humidity);
        _readings.Add(reading);
    }

    // status
    public void Activate()
    {
        if(Status == SensorStatus.Active)
            throw new DomainException("Senseor status at Active");

        if(Status == SensorStatus.Maintenance)
            throw new DomainException("Senseor status at Maintenance");

        Status = SensorStatus.Active;
        _domainEvents.Add(new SensorActivatedDomainEvent(Id, DateTime.UtcNow));
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

    public bool IsDeleted { get; private set; } = false;
    public void SoftDelete() => IsDeleted = true;
    bool IAuditableEntity.IsDeleted
    {
        get => IsDeleted;
        set => IsDeleted = value; 
    }

    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object>? DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}