// tests/AgriTech.Domain.UnitTests/Alerting/ThresholdExceededStrategyTests.cs
using Domain.Alerting;
using Domain.Alerting.Strategies;
using Domain.Sensors.Entities;
using Domain.Sensors.Enums;
using Domain.Sensors.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AgriTech.Domain.UnitTests.Alerting;

[Trait("Category", "Unit")]
public class ThresholdExceededStrategyTests
{
    private readonly ThresholdExceededStrategy _strategy;
    private readonly AlertContext _context;

    public ThresholdExceededStrategyTests()
    {
        _strategy = new ThresholdExceededStrategy(
            temperatureThreshold: 40.0,
            humidityThreshold: 85.0
        );
        _context = new AlertContext
        {
            SensorId = Guid.NewGuid(),
            SensorStatus = SensorStatus.Active
        };
    }

    [Fact]
    public async Task EvaluateAsync_WhenTemperatureExceedsThreshold_TriggersAlert()
    {
        // Arrange
        var reading = new SensorReading(
            DateTime.UtcNow,
            Temperature.FromCelsius(42.5),
            Humidity.FromPercentage(60.0)
        );

        // Act
        var result = await _strategy.EvaluateAsync(reading, _context, CancellationToken.None);

        // Assert
        result.IsAlertTriggered.Should().BeTrue();
        result.AlertMessage.Should().Contain("42.5");
        result.AlertMessage.Should().Contain("40");
        result.StrategyName.Should().Be(nameof(ThresholdExceededStrategy));
        result.Severity.Should().Be(AlertSeverity.Error);
        result.ThresholdValue.Should().Be(40.0);
        result.ActualValue.Should().Be(42.5);
    }

    [Fact]
    public async Task EvaluateAsync_WhenHumidityExceedsThreshold_TriggersAlert()
    {
        // Arrange
        var reading = new SensorReading(
            DateTime.UtcNow,
            Temperature.FromCelsius(30.0),
            Humidity.FromPercentage(90.0)
        );

        // Act
        var result = await _strategy.EvaluateAsync(reading, _context, CancellationToken.None);

        // Assert
        result.IsAlertTriggered.Should().BeTrue();
        result.AlertMessage.Should().Contain("90");
        result.AlertMessage.Should().Contain("85");
        result.StrategyName.Should().Be(nameof(ThresholdExceededStrategy));
        result.Severity.Should().Be(AlertSeverity.Error);
        result.ThresholdValue.Should().Be(85.0);
        result.ActualValue.Should().Be(90.0);
    }

    [Fact]
    public async Task EvaluateAsync_WhenValuesNormal_DoesNotTriggerAlert()
    {
        // Arrange
        var reading = new SensorReading(
            DateTime.UtcNow,
            Temperature.FromCelsius(25.0),
            Humidity.FromPercentage(50.0)
        );

        // Act
        var result = await _strategy.EvaluateAsync(reading, _context, CancellationToken.None);

        // Assert
        result.IsAlertTriggered.Should().BeFalse();
        result.AlertMessage.Should().BeNull();
    }

    [Theory]
    [InlineData(40.0, true)]
    [InlineData(39.9, false)]
    public async Task EvaluateAsync_AtTemperatureBoundary_BehavesCorrectly(double temperature, bool shouldAlert)
    {
        // Arrange
        var reading = new SensorReading(
            DateTime.UtcNow,
            Temperature.FromCelsius(temperature),
            Humidity.FromPercentage(50.0)
        );

        // Act
        var result = await _strategy.EvaluateAsync(reading, _context, CancellationToken.None);

        // Assert
        result.IsAlertTriggered.Should().Be(shouldAlert);
    }
}