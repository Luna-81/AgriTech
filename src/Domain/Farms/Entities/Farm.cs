using Domain.Common.Exceptions;
using Domain.Shared.ValueObjects;
using Domain.Common.Interfaces;
using Domain.Sensors.Entities;

namespace Domain.Farms.Entities;

public class Farm : IAggregateRoot, IAuditableEntity
{
    // 1. 实体标识与属性
    public Guid Id { get; private init; }
    public  string Name { get; private set; } = null!;
    public  Location Location { get; private set; } = null!;
    public int MaxSensorCapacity { get; private set; }

    // 审计属性 (IAuditableEntity)
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // 软删除状态
    public bool IsDeleted { get; private set; } = false;


    // 2. 集合与领域事件 (IAggregateRoot)
    private readonly List<Sensor> _sensors = new();
    public IReadOnlyList<Sensor> Sensors => _sensors.AsReadOnly();

    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object>? DomainEvents => _domainEvents.AsReadOnly();

    // EF Core 所需的构造函数
    private Farm() { }

    // 3. 构造函数
    private Farm(Guid id, string name, Location location, int maxCapacity)
    {
        Id = id;
        Name = name;
        Location = location;
        MaxSensorCapacity = maxCapacity;
    }

    // 4. 工厂方法
    public static Farm Create(string name, Location location, int maxCapacity = 50)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Farm name cannot be null");
        
        if (maxCapacity < 1 || maxCapacity > 100)
            throw new DomainException("Farm sensor capacity must be between 1 and 100");

        return new Farm(Guid.NewGuid(), name.Trim(), location, maxCapacity);
    }

    // 5. 业务逻辑
    public void AddSensor(Sensor sensor)
    {
        if (_sensors.Count >= MaxSensorCapacity)
            throw new DomainException($"Farm capacity already max ({MaxSensorCapacity}), can't add more sensors.");

        if (_sensors.Any(s => s.Id == sensor.Id))
            throw new DomainException("Sensor already exists in this Farm.");

        _sensors.Add(sensor);
    }

    public void SoftDelete() => IsDeleted = true;

    // 6. 接口实现
    public void ClearDomainEvents() => _domainEvents.Clear();

    bool IAuditableEntity.IsDeleted 
    { 
        get => IsDeleted; 
        set => IsDeleted = value; 
    }

}