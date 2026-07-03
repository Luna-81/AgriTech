using MassTransit;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using AgriTech.Worker.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgriTech.Worker; 
using Application; 

var builder = Host.CreateApplicationBuilder(args);

// ============ 1. 配置日志 ============
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ============ 从环境变量构建连接字符串 ============
var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") 
    ?? throw new InvalidOperationException("POSTGRES_HOST environment variable is not set");
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") 
    ?? throw new InvalidOperationException("POSTGRES_PORT environment variable is not set");
var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB") 
    ?? throw new InvalidOperationException("POSTGRES_DB environment variable is not set");
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") 
    ?? throw new InvalidOperationException("POSTGRES_USER environment variable is not set");
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") 
    ?? throw new InvalidOperationException("POSTGRES_PASSWORD environment variable is not set");

var connectionString = $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword};Pooling=true;Maximum Pool Size=100;";

Console.WriteLine($"✅ PostgreSQL: Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password=****");

// 覆盖配置
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// ============ 2. 注册服务 ============
builder.Services.AddApplication();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ============ 3. 添加 MassTransit ============
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SensorRegisteredConsumer>();
    x.AddConsumer<SensorReadingBatchConsumer>();
    x.AddConsumer<AlertNotificationConsumer>();
    x.AddConsumer<DeadLetterConsumer>();

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") 
            ?? throw new InvalidOperationException("RABBITMQ_HOST environment variable is not set");
        var username = Environment.GetEnvironmentVariable("RABBITMQ_USER") 
            ?? throw new InvalidOperationException("RABBITMQ_USER environment variable is not set");
        var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") 
            ?? throw new InvalidOperationException("RABBITMQ_PASSWORD environment variable is not set");

        Console.WriteLine($"✅ RabbitMQ: Host={host};Username={username};Password=****");

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ReceiveEndpoint("sensor-registered-queue", e =>
        {
            e.ConfigureConsumer<SensorRegisteredConsumer>(context);
            e.PrefetchCount = 5;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
        });

        cfg.ReceiveEndpoint("sensor-reading-batch-queue", e =>
        {
            e.ConfigureConsumer<SensorReadingBatchConsumer>(context);
            e.PrefetchCount = 50;
            e.UseMessageRetry(r => r.Interval(2, TimeSpan.FromSeconds(3)));
        });

        cfg.ReceiveEndpoint("alert-notification-queue", e =>
        {
            e.ConfigureConsumer<AlertNotificationConsumer>(context);
            e.PrefetchCount = 10;
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(1)));
        });

        cfg.ReceiveEndpoint("dead-letter-queue", e =>
        {
            e.Consumer<DeadLetterConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// ============ 4. 注册健康检查 ============
builder.Services.AddHealthChecks();

// ============ 5. 注册后台服务 ============
builder.Services.AddHostedService<Worker>();

// ============ 6. 添加内存缓存 ============
builder.Services.AddMemoryCache();

var app = builder.Build();

// 启动应用
await app.RunAsync();