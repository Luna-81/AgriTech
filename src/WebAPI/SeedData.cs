using Infrastructure.Persistence;
using Domain.Farms.Entities;
using Domain.Sensors.Entities;
using Domain.Shared.ValueObjects;
using Domain.Sensors.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace WebAPI;

/// <summary>
/// 种子数据初始化
/// 用于在应用启动时填充测试数据
/// </summary>
public static class SeedData
{
    /// <summary>
    /// 初始化种子数据
    /// 如果数据库已有数据，则跳过初始化
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (await context.Farms.AnyAsync())
            return;

        // ============ 创建农场 ============
        var farm = Farm.Create(
            "AgriTech Demo Farm",
            Location.FromCoordinates(31.2304, 121.4737),
            maxCapacity: 10);

        // ============ 创建传感器 ============
        var sensor1 = Sensor.Create(
            "温室温度传感器 A",
            Temperature.FromCelsius(35),
            Location.FromCoordinates(31.2304, 121.4737));
        farm.AddSensor(sensor1);

        var sensor2 = Sensor.Create(
            "大田湿度传感器 B",
            Temperature.FromCelsius(40),
            Location.FromCoordinates(31.2400, 121.4800));
        farm.AddSensor(sensor2);

        // ============ 激活传感器 ============
        sensor1.Activate();
        sensor2.Activate();

        // ============ 添加历史数据 ============
        // 使用 SensorReading 方法添加历史数据
        sensor1.SensorReading(33.5, 68.2, DateTime.UtcNow.AddHours(-2));
        sensor1.SensorReading(34.2, 65.5, DateTime.UtcNow.AddHours(-1));
        sensor1.SensorReading(36.8, 62.3, DateTime.UtcNow.AddMinutes(-30));
        sensor1.SensorReading(35.1, 64.7, DateTime.UtcNow.AddMinutes(-15));
        
        sensor2.SensorReading(24.5, 80.1, DateTime.UtcNow.AddHours(-3));
        sensor2.SensorReading(25.1, 78.9, DateTime.UtcNow.AddMinutes(-45));
        sensor2.SensorReading(24.8, 79.5, DateTime.UtcNow.AddMinutes(-20));

        // ============ 保存到数据库 ============
        await context.Farms.AddAsync(farm);
        await context.SaveChangesAsync();
    }
}