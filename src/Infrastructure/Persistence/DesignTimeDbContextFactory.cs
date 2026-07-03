using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Infrastructure.Persistence;

namespace Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // ✅ 从环境变量读取连接字符串
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") 
            ?? throw new InvalidOperationException("POSTGRES_HOST environment variable is not set");
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") 
            ?? throw new InvalidOperationException("POSTGRES_PORT environment variable is not set");
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") 
            ?? throw new InvalidOperationException("POSTGRES_DB environment variable is not set");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") 
            ?? throw new InvalidOperationException("POSTGRES_USER environment variable is not set");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") 
            ?? throw new InvalidOperationException("POSTGRES_PASSWORD environment variable is not set");

        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};Pooling=true;Maximum Pool Size=100;";
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            npgsqlOptions.EnableRetryOnFailure(3);
        });

        return new AppDbContext(optionsBuilder.Options, null!, null!);
    }
}