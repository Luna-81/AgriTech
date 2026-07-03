using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.Logging;
using Domain.Common.Interfaces;
using Domain.Farms.RepositoryInterfaces;   
using Domain.Sensors.RepositoryInterfaces;
using Npgsql;

namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 1. 获取连接字符串
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not found.");

        // ✅ 只在开发环境输出连接字符串（用于调试）
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (environment == "Development")
        {
            // ⚠️ 只输出部分信息，隐藏密码
            var maskedConn = MaskConnectionString(connectionString);
            Console.WriteLine($"🔍 连接字符串: {maskedConn}");
        }

        // ✅ 移除直接连接测试（或在开发环境保留）
        if (environment == "Development")
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                Console.WriteLine("✅ Npgsql 直接连接成功！");
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Npgsql 直接连接失败: {ex.Message}");
            }
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.UseNetTopologySuite(); 
            });

            // 开发环境日志配置
            if (environment == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
        });

        // 2. 注册 IUnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // 3. 注册仓储 (使用 Scrutor 自动扫描)
        services.Scan(scan => scan
            .FromAssemblyOf<AppDbContext>()
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        services.AddScoped<IFarmRepository, FarmRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();

        // 4. 注册健康检查
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("PostgreSQL", 
                failureStatus: HealthStatus.Degraded, 
                tags: new[] { "database" });

        return services;
    }

    // ✅ 辅助方法：隐藏连接字符串中的密码
    private static string MaskConnectionString(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.Password))
            {
                builder.Password = "****";
            }
            return builder.ToString();
        }
        catch
        {
            // 如果解析失败，返回通用提示
            return "Connection string (password hidden)";
        }
    }
}