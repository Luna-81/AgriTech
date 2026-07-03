// tests/AgriTech.Domain.UnitTests/Entities/DataFactorySensorTests.cs
using Domain.Farms.Entities;
using Domain.Sensors.Entities;
using Domain.Sensors.Enums;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AgriTech.Domain.UnitTests.Entities;

public class DataFactorySensorTests
{
    [Fact]
    public void CreateSensor_ReturnsValidSensor()
    {
        // Arrange
        var sensor = Sensor.Create(
            "测试传感器",
            Temperature.FromCelsius(35),
            Location.FromCoordinates(31.23, 121.47)
        );

        // Act
        sensor.Activate();

        // Assert
        sensor.Should().NotBeNull();
        sensor.Id.Should().NotBeEmpty();
        sensor.Status.Should().Be(SensorStatus.Active);
        sensor.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateFarm_ReturnsValidFarm()
    {
        // Arrange & Act
        var farm = Farm.Create(
            "测试农场",
            Location.FromCoordinates(31.23, 121.47),
            10
        );

        // Assert
        farm.Should().NotBeNull();
        farm.Id.Should().NotBeEmpty();
        farm.Name.Should().Be("测试农场");
        farm.MaxSensorCapacity.Should().Be(10);
        farm.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Sensor_CanActivateAndDeactivate()
    {
        // Arrange
        var sensor = Sensor.Create(
            "测试传感器",
            Temperature.FromCelsius(35),
            Location.FromCoordinates(31.23, 121.47)
        );

        // Act - 激活
        sensor.Activate();
        sensor.Status.Should().Be(SensorStatus.Active);

        // Act - 停用
        sensor.Deactivate();
        sensor.Status.Should().Be(SensorStatus.Inactive);
    }

    [Fact]
    public void Farm_AddSensor_AddsSensorToFarm()
    {
        // Arrange
        var farm = Farm.Create("测试农场", Location.FromCoordinates(31.23, 121.47), 5);
        var sensor = Sensor.Create(
            "测试传感器",
            Temperature.FromCelsius(35),
            Location.FromCoordinates(31.23, 121.47)
        );
        sensor.Activate();

        // Act
        farm.AddSensor(sensor);

        // Assert
        farm.Sensors.Should().Contain(sensor);
        farm.Sensors.Count.Should().Be(1);
    }

    [Fact]
    public void Sensor_RecordReading_AddsReading()
    {
        // Arrange
        var sensor = Sensor.Create(
            "测试传感器",
            Temperature.FromCelsius(35),
            Location.FromCoordinates(31.23, 121.47)
        );
        sensor.Activate();

        // Act
        sensor.SensorReading(25.5, 60.0, DateTime.UtcNow);

        // Assert
        sensor.Readings.Should().HaveCount(1);
    }
}