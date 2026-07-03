// tests/AgriTech.Application.UnitTests/TestHelpers/Customizations.cs
using AutoFixture;
using Domain.Farms.Entities;
using Domain.Sensors.Entities;
using Domain.Sensors.ValueObjects;
using Domain.Shared.ValueObjects;

namespace AgriTech.Application.UnitTests.TestHelpers;

/// <summary>
/// Domain 实体自定义生成器
/// 用于 AutoFixture 自动生成有效的 Domain 实体
/// </summary>
public class DomainEntityCustomization:ICustomization
{
    public void Customize(IFixture fixture)
    {
        // 为 Sensor 创建自定义构建器
        fixture.Customize<Sensor>(composer => composer.FromFactory(()=> 
        {
            var name = fixture.Create<string>();
            while(string.IsNullOrWhiteSpace(name))
            {
                name = fixture.Create<string>();
            }

            var temperature = fixture.Create<double>();
            while(temperature < -50 || temperature >80)
            {
                temperature = fixture.Create<double>();
            }

            var lat = fixture.Create<double>() % 90;
            var lon = fixture.Create<double>() %180;

            return Sensor.Create(name, 
                Temperature.FromCelsius(temperature),
                Location.FromCoordinates(lat,lon));

        }));

        // 为 Farm 创建自定义构建器
        fixture.Customize<Farm>(composer =>
            composer.FromFactory(() =>
            {
                var name = fixture.Create<string>();
                while (string.IsNullOrWhiteSpace(name))
                {
                    name = fixture.Create<string>();
                }
                
                var lat = fixture.Create<double>() % 90;
                var lon = fixture.Create<double>() % 180;
                var capacity = fixture.Create<int>() % 100 + 1; // 1-100
                
                return Farm.Create(
                    name,
                    Location.FromCoordinates(lat, lon),
                    capacity
                );
            }));      
    }
    
}

/// <summary>
/// 测试数据生成器工厂
/// 提供预配置的 Fixture 实例
/// </summary>
public static class TestDataFactory
{
    /// <summary>
    /// 创建带有 Domain 自定义的 Fixture
    /// </summary>
    public static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize(new DomainEntityCustomization());
        return fixture;
    }

    /// <summary>
    /// 创建一个测试 Sensor
    /// </summary>
    public static Sensor CreateTestSensor()
    {
        var fixture = CreateFixture();
        var sensor = fixture.Create<Sensor>();
        
        // 激活传感器以便测试读数
        sensor.Activate();
        return sensor;
    }

    /// <summary>
    /// 创建一个测试 Farm
    /// </summary>
    public static Farm CreateTestFarm(int capacity = 10)
    {
        var fixture = CreateFixture();
        return Farm.Create(
            fixture.Create<string>(),
            Location.FromCoordinates(
                fixture.Create<double>() % 90,
                fixture.Create<double>() % 180
            ),
            capacity
        );
    }

}