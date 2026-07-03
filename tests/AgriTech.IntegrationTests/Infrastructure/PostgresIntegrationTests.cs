using System;
using Domain.Farms.Entities;
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;
using Domain.Sensors.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AgriTech.IntegrationTests.Infrastructure;

[Trait("Category", "Integration")]
public class PostgresIntegrationTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly string _connectionString;

    public PostgresIntegrationTests()
    {
        // ✅ 从环境变量读取，如果不存在则抛出异常
        _connectionString = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING") 
            ?? throw new InvalidOperationException(
                "TEST_CONNECTION_STRING environment variable is not set. " +
                "Please set it before running integration tests. " +
                "Refer to .env.example for configuration.");
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        _dbContext = new AppDbContext(options, null!, null!);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    [Fact]
    public async Task CanConnectToDatabase()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task AddFarm_WithValidData_PersistsToDatabase()
    {
        var farm = Farm.Create("集成测试农场", Location.FromCoordinates(31.23, 121.47), 10);
        await _dbContext.Farms.AddAsync(farm);
        await _dbContext.SaveChangesAsync();

        var savedFarm = await _dbContext.Farms.FindAsync(farm.Id);
        savedFarm.Should().NotBeNull();
        savedFarm!.Name.Should().Be("集成测试农场");
    }

    [Fact]
    public async Task AddSensor_WithValidData_PersistsToDatabase()
    {
        var farm = Farm.Create("农场", Location.FromCoordinates(31.23, 121.47), 10);
        await _dbContext.Farms.AddAsync(farm);
        await _dbContext.SaveChangesAsync();

        var sensor = Sensor.Create("测试传感器", Temperature.FromCelsius(35), Location.FromCoordinates(31.23, 121.47));
        await _dbContext.Sensors.AddAsync(sensor);
        await _dbContext.SaveChangesAsync();

        var savedSensor = await _dbContext.Sensors.FindAsync(sensor.Id);
        savedSensor.Should().NotBeNull();
        savedSensor!.Name.Should().Be("测试传感器");
        savedSensor.Status.Should().Be(SensorStatus.Inactive);
    }
}