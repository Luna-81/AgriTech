// tests/AgriTech.Domain.UnitTests/Alerting/AlertEngineTests.cs
using Domain.Alerting;
using Domain.Alerting.Strategies;
using Domain.Sensors.Entities;
using Domain.Sensors.Enums;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AgriTech.Domain.UnitTests.Alerting;

[Trait("Category", "Unit")]
public class AlertEngineTests
{
    private readonly List<IAlertStrategy> _strategies;

    public AlertEngineTests()
    {
        _strategies = new List<IAlertStrategy>
        {
            new ThresholdExceededStrategy(temperatureThreshold: 40.0, humidityThreshold: 85.0),
            new ConsecutiveExceededStrategy(requiredConsecutiveCount: 3, temperatureThreshold: 38.0)
        };
    }

    [Fact]
    public async Task EvaluateReadingsAsync_WithMultipleReadings_ReturnsCorrectAlerts()
    {
        // Arrange
        var engine = new AlertEngine(_strategies);
        var sensor = CreateTestSensor();
        var readings = new List<SensorReading>
        {
            new(DateTime.UtcNow.AddMinutes(-10), Temperature.FromCelsius(25.0), Humidity.FromPercentage(60.0)),
            new(DateTime.UtcNow.AddMinutes(-5), Temperature.FromCelsius(42.0), Humidity.FromPercentage(60.0)),
            new(DateTime.UtcNow, Temperature.FromCelsius(30.0), Humidity.FromPercentage(90.0))
        };

        // Act
        var results = await engine.EvaluateReadingsAsync(sensor, readings);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.AlertMessage!.Contains("42.0"));
        results.Should().Contain(r => r.AlertMessage!.Contains("90.0"));
    }

    [Fact]
    public async Task EvaluateReadingsAsync_WithEmptyReadings_ReturnsEmptyResults()
    {
        // Arrange
        var engine = new AlertEngine(_strategies);
        var sensor = CreateTestSensor();
        var readings = new List<SensorReading>();

        // Act
        var results = await engine.EvaluateReadingsAsync(sensor, readings);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateReadingsAsync_WithConsecutiveExceed_TriggersAlert()
    {
        // Arrange
        var engine = new AlertEngine(_strategies);
        var sensor = CreateTestSensor();
        var now = DateTime.UtcNow;
        var readings = new List<SensorReading>
        {
            new(now.AddMinutes(-10), Temperature.FromCelsius(39.0), Humidity.FromPercentage(60.0)),
            new(now.AddMinutes(-5), Temperature.FromCelsius(38.5), Humidity.FromPercentage(60.0)),
            new(now, Temperature.FromCelsius(39.5), Humidity.FromPercentage(60.0))
        };

        // Act
        var results = await engine.EvaluateReadingsAsync(sensor, readings);

        // Assert
        results.Should().ContainSingle()
            .Which.AlertMessage.Should().Contain("连续 3 次");
    }

    private Sensor CreateTestSensor()
    {
        var sensor = Sensor.Create(
            "测试传感器",
            Temperature.FromCelsius(35),
            Location.FromCoordinates(31.23, 121.47)
        );
        sensor.Activate();
        return sensor;
    }
}