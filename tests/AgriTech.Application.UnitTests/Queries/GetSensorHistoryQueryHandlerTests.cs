// tests/AgriTech.Application.UnitTests/Queries/GetSensorHistoryQueryHandlerTests.cs
using Application.Features.Sensors.Queries.GetSensorHistory;
using Application.Features.Sensors.DTOs;
using Domain.Common.Interfaces;
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;
using AutoFixture;

namespace AgriTech.Application.UnitTests.Queries;

public class GetSensorHistoryQueryHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IRepository<Sensor>> _sensorRepositoryMock;
    private readonly GetSensorHistoryQueryHandler _handler;

    public GetSensorHistoryQueryHandlerTests()
    {
        _fixture = new Fixture();
        _sensorRepositoryMock = new Mock<IRepository<Sensor>>();
        _handler = new GetSensorHistoryQueryHandler(_sensorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSensorNotFound_ReturnsFailure()
    {
        // Arrange
        var query = new GetSensorHistoryQuery
        {
            SensorId = Guid.NewGuid(),
            Page = 1,
            PageSize = 10
        };

        _sensorRepositoryMock
            .Setup(x => x.GetByIdAsync(query.SensorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sensor?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain($"The sensor with ID {query.SensorId} was not found.");
    }

    [Fact]
    public async Task Handle_WithValidSensorId_ReturnsReadings()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = CreateSensorWithReadings(sensorId, 25);
        var query = new GetSensorHistoryQuery
        {
            SensorId = sensorId,
            Page = 1,
            PageSize = 10
        };

        _sensorRepositoryMock
            .Setup(x => x.GetByIdAsync(sensorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
        result.Value.Should().AllBeOfType<SensorReadingDto>();
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = CreateSensorWithReadings(sensorId, 25);
        var query = new GetSensorHistoryQuery
        {
            SensorId = sensorId,
            Page = 2,
            PageSize = 10
        };

        _sensorRepositoryMock
            .Setup(x => x.GetByIdAsync(sensorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_WithDateFilter_ReturnsFilteredReadings()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = CreateSensorWithReadings(sensorId, 25);
        var startDate = DateTime.UtcNow.AddHours(-2);
        var endDate = DateTime.UtcNow.AddHours(-1);
        var query = new GetSensorHistoryQuery
        {
            SensorId = sensorId,
            StartDate = startDate,
            EndDate = endDate,
            Page = 1,
            PageSize = 10
        };

        _sensorRepositoryMock
            .Setup(x => x.GetByIdAsync(sensorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // 所有读数应该在时间范围内
        foreach (var reading in result.Value!)
        {
            reading.Timestamp.Should().BeOnOrAfter(startDate);
            reading.Timestamp.Should().BeOnOrBefore(endDate);
        }
    }

    [Fact]
    public async Task Handle_WhenReadingCountLessThanPageSize_ReturnsAllReadings()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var sensor = CreateSensorWithReadings(sensorId, 5);
        var query = new GetSensorHistoryQuery
        {
            SensorId = sensorId,
            Page = 1,
            PageSize = 10
        };

        _sensorRepositoryMock
            .Setup(x => x.GetByIdAsync(sensorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    private Sensor CreateSensorWithReadings(Guid sensorId, int count)
    {
        var sensor = Sensor.Create(
            _fixture.Create<string>(),
            Temperature.FromCelsius(35),
            Location.FromCoordinates(31.23, 121.47)
        );
        
        // 使用反射设置 Id
        typeof(Sensor).GetProperty("Id")?.SetValue(sensor, sensorId);
        sensor.Activate();

        for (int i = 0; i < count; i++)
        {
            sensor.SensorReading(
                25 + i * 0.5,
                60 + i * 0.3,
                DateTime.UtcNow.AddMinutes(-i * 5)
            );
        }

        return sensor;
    }
}