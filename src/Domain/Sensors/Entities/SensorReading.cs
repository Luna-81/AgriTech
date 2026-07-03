// src/Domain/Sensors/Entities/SensorReading.cs
using Domain.Sensors.ValueObjects;

namespace Domain.Sensors.Entities;

public record SensorReading(
    DateTime Timestamp,
    Temperature Temperature,
    Humidity Humidity
);