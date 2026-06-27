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
 


namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 1. 注册 DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not found.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.UseNetTopologySuite(); 

            });

            // 开发环境日志配置
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
        });

        // 2. 注册 IUnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // 3. 注册仓储 (使用 Scrutor 自动扫描)
        services.Scan(scan => scan
            .FromAssemblyOf<AppDbContext>() // 扫描 Infrastructure 程序集
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        // 如果你觉得 Scrutor 调试太麻烦，也可以直接使用这行替代：
        // services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddScoped<IFarmRepository, FarmRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();

        // 4. 注册健康检查
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("PostgreSQL", 
                failureStatus: HealthStatus.Degraded, 
                tags: new[] { "database" });

        return services;
    }
}