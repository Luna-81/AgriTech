// tests/AgriTech.Domain.UnitTests/Alerting/RateOfChangeStrategyTests.cs
using Domain.Alerting;
using Domain.Alerting.Strategies;
using Domain.Sensors.Entities;
using Domain.Sensors.Enums;
using Domain.Sensors.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AgriTech.Domain.UnitTests.Alerting;

[Trait("Category", "Unit")]
public class RateOfChangeStrategyTests
{
    private readonly RateOfChangeStrategy _strategy;
    private readonly AlertContext _context;

    public RateOfChangeStrategyTests()
    {
        _strategy = new RateOfChangeStrategy(maxRatePerSecond: 0.5);
        _context = new AlertContext
        {
            SensorId = Guid.NewGuid(),
            SensorStatus = SensorStatus.Active
        };
    }

    [Fact]
    public async Task EvaluateAsync_WhenRateExceedsThreshold_TriggersAlert()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var reading1 = new SensorReading(
            now,
            Temperature.FromCelsius(20.0),
            Humidity.FromPercentage(50.0)
        );
        await _strategy.EvaluateAsync(reading1, _context, CancellationToken.None);

        var reading2 = new SensorReading(
            now.AddSeconds(5),
            Temperature.FromCelsius(30.0),
            Humidity.FromPercentage(50.0)
        );

        // Act
        var result = await _strategy.EvaluateAsync(reading2, _context, CancellationToken.None);

        // Assert
        result.IsAlertTriggered.Should().BeTrue();
        result.AlertMessage.Should().Contain("变化率异常");
        result.Severity.Should().Be(AlertSeverity.Critical);
    }

    [Fact]
    public async Task EvaluateAsync_WhenRateBelowThreshold_DoesNotTriggerAlert()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var reading1 = new SensorReading(
            now,
            Temperature.FromCelsius(20.0),
            Humidity.FromPercentage(50.0)
        );
        await _strategy.EvaluateAsync(reading1, _context, CancellationToken.None);

        var reading2 = new SensorReading(
            now.AddSeconds(10),
            Temperature.FromCelsius(21.0),
            Humidity.FromPercentage(50.0)
        );

        // Act
        var result = await _strategy.EvaluateAsync(reading2, _context, CancellationToken.None);

        // Assert
        result.IsAlertTriggered.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_FirstReading_StoresValueAndDoesNotAlert()
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
        _context.PreviousTemperature.Should().Be(25.0);
        _context.PreviousTemperatureTime.Should().NotBeNull();
    }
}