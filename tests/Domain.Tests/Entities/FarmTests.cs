using Domain.Farms.Entities;      
using Domain.Sensors.Entities;  
using Domain.Common.Exceptions;
using Domain.Shared.ValueObjects;
using Domain.Sensors.ValueObjects;
using Xunit;

namespace Domain.Tests.Entities;

public class FarmTests
{
    [Fact]
    public void Create_ValidFarm_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var location = Location.FromCoordinates(31.23,121.47);
        var farm = Farm.Create("Green Valley Farm", location, 10);

        //Assert

        Assert.Equal("Green Valley Farm", farm.Name);
        Assert.Equal(10,farm.MaxSensorCapacity);
        Assert.Empty(farm.Sensors);
    }

    [Fact]
    public void AddSensor_ValidSensor_ShouldIncreaseCount()
    {
        // Arrange
        var farm = Farm.Create("Test Farm", Location.FromCoordinates(0, 0), 5);
        var sensor = Sensor.Create("Sensor-01", Temperature.FromCelsius(25), Location.FromCoordinates(0, 0));

        // Act
        farm.AddSensor(sensor);

        // Assert
        Assert.Single(farm.Sensors);
        Assert.Equal(sensor.Id, farm.Sensors[0].Id);
    }

    [Theory]
    [InlineData(1,2)]
    [InlineData(5,6)]
    public void AddSensor_WhenExceedingCapacity_ShouldThrowDomainException(int capacity, int count)
    {
        // Arrange
        var farm = Farm.Create("Limit Test Farm",Location.FromCoordinates(0, 0), capacity);

        //Act & Assert
        for (int i = 0; i < count; i++)
        {
            var sensor = Sensor.Create($"S{i}", Temperature.FromCelsius(20), Location.FromCoordinates(0, 0));

            if (i < capacity)
            {
                farm.AddSensor(sensor);
            }
            else
            {
                Assert.Throws<DomainException>(() => farm.AddSensor(sensor));
            }
        }

    }


    [Fact]
    public void AddSensor_WhenAddingDuplicateId_ShouldThrowDomainException()
    {
        // Arrange
        var farm = Farm.Create("Unique Farm", Location.FromCoordinates(0, 0), 5);
        var sensor = Sensor.Create("S1", Temperature.FromCelsius(20), Location.FromCoordinates(0, 0));
        farm.AddSensor(sensor);

        // Act & Assert
        Assert.Throws<DomainException>(() => farm.AddSensor(sensor));
    }

    [Fact]
    public void Sensors_WhenAccessingReadonlyList_ShouldNotAllowDirectModification()
    {
        var farm = Farm.Create("Test Farm", Location.FromCoordinates(0, 0), 5);
        Assert.IsAssignableFrom<IReadOnlyList<Sensor>>(farm.Sensors);
    }

}
