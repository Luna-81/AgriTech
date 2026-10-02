// tests/AgriTech.Application.UnitTests/Commands/RegisterSensorCommandHandlerTests.cs
using Application.Common.Interfaces;
using Application.Features.Sensors.Commands.RegisterSensor;
using Application.Events;
using Domain.Farms.Entities;
using Domain.Farms.RepositoryInterfaces;
using Domain.Shared.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AutoFixture;

namespace AgriTech.Application.UnitTests.Commands;

[Trait("Category", "Unit")]
public class RegisterSensorCommandHandlerTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IFarmRepository> _farmRepositoryMock;
    private readonly Mock<IEventPublisher> _publisherMock;      
    private readonly Mock<ILogger<RegisterSensorCommandHandler>> _loggerMock;
    private readonly RegisterSensorCommandHandler _handler;

    public RegisterSensorCommandHandlerTests()
    {
        _fixture = new Fixture();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _publisherMock = new Mock<IEventPublisher>();            
        _loggerMock = new Mock<ILogger<RegisterSensorCommandHandler>>();

        _handler = new RegisterSensorCommandHandler(
            _farmRepositoryMock.Object,
            _publisherMock.Object,                              
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessAndPublishesEvent()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var farm = Farm.Create("Test Farm", Location.FromCoordinates(31.23, 121.47), 10);
        var command = new RegisterSensorCommand
        {
            Name = "Test Sensor",
            TemperatureThreshold = 35.0,
            Latitude = 31.23,
            Longitude = 121.47,
            FarmId = farmId
        };

        _farmRepositoryMock
            .Setup(x => x.GetByIdAsync(farmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(farm);

        _publisherMock
            .Setup(x => x.PublishAsync(
                It.IsAny<SensorRegisteredEvent>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _publisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<SensorRegisteredEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFarmNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterSensorCommand
        {
            Name = "Test Sensor",
            TemperatureThreshold = 35.0,
            Latitude = 31.23,
            Longitude = 121.47,
            FarmId = Guid.NewGuid()
        };

        _farmRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Farm?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainMatch("*不存在*");

        _publisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<SensorRegisteredEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterSensorCommand
        {
            Name = "", // unvalid name to trigger domain exception
            TemperatureThreshold = 35.0,
            Latitude = 31.23,
            Longitude = 121.47,
            FarmId = Guid.NewGuid()
        };

        var farm = Farm.Create("Test Farm", Location.FromCoordinates(31.23, 121.47), 10);
        _farmRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(farm);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _publisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<SensorRegisteredEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPublishFails_ReturnsFailure()
    {
        // Arrange
        var farmId = Guid.NewGuid();
        var farm = Farm.Create("Test Farm", Location.FromCoordinates(31.23, 121.47), 10);
        var command = new RegisterSensorCommand
        {
            Name = "Test Sensor",
            TemperatureThreshold = 35.0,
            Latitude = 31.23,
            Longitude = 121.47,
            FarmId = farmId
        };

        _farmRepositoryMock
            .Setup(x => x.GetByIdAsync(farmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(farm);

        _publisherMock
            .Setup(x => x.PublishAsync(
                It.IsAny<SensorRegisteredEvent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message broker connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _publisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<SensorRegisteredEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}