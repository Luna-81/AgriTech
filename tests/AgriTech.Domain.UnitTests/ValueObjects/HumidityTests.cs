// tests/AgriTech.Domain.UnitTests/ValueObjects/HumidityTests.cs
using Domain.Sensors.ValueObjects;
using Domain.Common.Exceptions;
using FluentAssertions;
using Xunit;

namespace AgriTech.Domain.UnitTests.ValueObjects;

public class HumidityTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(90)]
    public void FromPercentage_WithValidValue_ReturnsHumidity(double percentage)
    {
        //Arrange & Act
        var humidity = Humidity.FromPercentage(percentage);
        //Assert
        humidity.Percentage.Should().Be(percentage);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(91)]
    public void FromPercentage_WithInvalidValue_ThrowsDomainException(double percentage)
    {
        //Arrange 
        var act = () => Humidity.FromPercentage(percentage);
        
        //Act & Assert
        act.Should().Throw<DomainException>().WithMessage($"Humidity not in 10 to 90");

    }
    [Fact]
    public void FromPercentage_RoundsToTwoDecimalPlaces()
    {
        //Arrange
        var percentage = 65.555;

        var humidity = Humidity.FromPercentage(percentage);

        // Assert
        humidity.Percentage.Should().Be(65.56);

    }

    [Fact]
    public void FromPercentage_WithValidValue_ReturnsHumidityObject()
    {
        // Arrange
        var percentage = 65.5;

        // Act
        var humidity = Humidity.FromPercentage(percentage);

        // Assert
        humidity.Should().NotBeNull();
        humidity.Percentage.Should().Be(percentage);
    }
}