// tests/AgriTech.Domain.UnitTests/ValueObjects/LocationTests.cs
using Domain.Shared.ValueObjects;
using Domain.Common.Exceptions;
using FluentAssertions;
using Xunit;
namespace AgriTech.Domain.UnitTests.ValueObjects;

public class LocationTests
{
    
    [Theory]
    [InlineData(31.2304, 121.4737, 31.2400, 121.4800, 1.1)]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(90, 180, -90, -180, 20015)]
    public void DistanceTo_WithTwoLocations_ReturnsCorrectDistance(
        double lat1, double lon1, double lat2, double lon2, double expectedDistance)
    {
        // Arrange
        var loc1 = Location.FromCoordinates(lat1, lon1);
        var loc2 = Location.FromCoordinates(lat2, lon2);

        // Act
        var distance = loc1.DistanceTo(loc2);

        // Assert
        distance.Should().BeApproximately(expectedDistance, 1.0);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void FromCoordinates_WithInvalidLatitude_ThrowsDomainException(double latitude)
    {
        // Arrange
        var act = () => Location.FromCoordinates(latitude, 121.0);

        // Act & Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"latitude {latitude} not in -90 to 90");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void FromCoordinates_WithInvalidLongitude_ThrowsDomainException(double longitude)
    {
        // Arrange
        var act = () => Location.FromCoordinates(31.0, longitude);

        // Act & Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"longitude {longitude} not in -180 to 180");
    }

    [Fact]
    public void FromCoordinates_WithValidCoordinates_CreatesLocation()
    {
        // Arrange
        var latitude = 31.2304;
        var longitude = 121.4737;

        // Act
        var location = Location.FromCoordinates(latitude, longitude);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Equals_WithSameCoordinates_ReturnsTrue()
    {
        // Arrange
        var loc1 = Location.FromCoordinates(31.2304, 121.4737);
        var loc2 = Location.FromCoordinates(31.2304, 121.4737);

        // Act & Assert
        loc1.Should().Be(loc2);
        (loc1 == loc2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var loc1 = Location.FromCoordinates(31.2304, 121.4737);
        var loc2 = Location.FromCoordinates(31.2400, 121.4800);

        // Act & Assert
        loc1.Should().NotBe(loc2);
        (loc1 == loc2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsFormattedCoordinates()
    {
        // Arrange
        var location = Location.FromCoordinates(31.2304, 121.4737);

        // Act
        var result = location.ToString();

        // Assert
        result.Should().Be("Location { Latitude = 31.2304, Longitude = 121.4737 }");
    }

}
