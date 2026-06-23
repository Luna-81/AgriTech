using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Farm
{
    public Guid Id{get; private init;}
    public string Name{get; private set;}
    public Location Location{get;private set;}
    public int MaxSensorCapacity{get; private set;}

    private readonly List<Sensor> _sensors = new();

    public IReadOnlyList<Sensor> Sensors => _sensors.AsReadOnly();
    private Farm(Guid id, string name, Location location, int maxCapacity)
    {
        Id = id;
        Name = name;
        Location = location;
        MaxSensorCapacity = maxCapacity;
    }

    public static Farm Create(string name, Location location,int maxCapacity = 50)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new DomainException("Farm name cannot null");
        
        if(maxCapacity <1 || maxCapacity >100)
            throw new DomainException("Farm sensor capacity must in 1 to 100");

        return new Farm(Guid.NewGuid(),name.Trim(),location, maxCapacity);

    }

    public void AddSensor(Sensor sensor)
    {
        if(_sensors.Count >= MaxSensorCapacity)
            throw new DomainException("Farm capacity already max ({MaxSensorCapacity}), can't add more sensors.");

        if(_sensors.Any(s=> s.Id == sensor.Id))
            throw new DomainException("sensor name exist in Fram");

        _sensors.Add(sensor);
    }


}