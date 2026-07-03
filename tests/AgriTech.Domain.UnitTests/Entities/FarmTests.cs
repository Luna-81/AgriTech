using Domain.Farms.Entities;
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using Domain.Common.Exceptions;
using FluentAssertions;
using Xunit;
namespace AgriTech.Domain.UnitTests.Entities;

[Trait("Category", "Unit")]
public class FarmTests
{
     [Fact]
    public void Create_WithValidData_ReturnsFarm()
    {
        // Arrange
        var name = "测试农场";
        var location = Location.FromCoordinates(31.23, 121.47);
        var maxCapacity = 50;

        // Act
        var farm = Farm.Create(name, location, maxCapacity);

        // Assert
        farm.Should().NotBeNull();
        farm.Id.Should().NotBeEmpty();
        farm.Name.Should().Be(name);
        farm.Location.Should().Be(location);
        farm.MaxSensorCapacity.Should().Be(maxCapacity);
        farm.Sensors.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmptyName_ThrowsDomainException()
    {
        // Arrange
        var name = "";
        var location = Location.FromCoordinates(31.23, 121.47);

        // Act
        Action act = () => Farm.Create(name, location, 50);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddSensor_WhenUnderCapacity_AddsSensor()
    {
        // Arrange
        var farm = Farm.Create("测试农场", Location.FromCoordinates(31.23, 121.47), 2);
        var sensor = CreateTestSensor();

        // Act
        farm.AddSensor(sensor);

        // Assert
        farm.Sensors.Should().Contain(sensor);
    }

    [Fact]
    public void AddSensor_WhenAtCapacity_ThrowsDomainException()
    {
        // Arrange
        var farm = Farm.Create("测试农场", Location.FromCoordinates(31.23, 121.47), 1);
        var sensor1 = CreateTestSensor();
        var sensor2 = CreateTestSensor();
        farm.AddSensor(sensor1);

        // Act
        Action act = () => farm.AddSensor(sensor2);

        // Assert
        act.Should().Throw<DomainException>();
    }


     private Sensor CreateTestSensor()
    {
        return Sensor.Create("测试传感器", Temperature.FromCelsius(35), Location.FromCoordinates(31.23, 121.47));
    }



}