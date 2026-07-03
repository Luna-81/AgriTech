using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using WebAPI;
using WebAPI.Exceptions;
using WebAPI.Filters;
using MassTransit;
using Domain.Common.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// ============ 配置日志 ============
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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

// ============ 注册服务 ============

// 1. 注册 Infrastructure 服务
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. 注册 Application 服务（MediatR）
builder.Services.AddApplication();

// 3. 注册 Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilterAttribute>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

// 4. 注册 API 版本控制
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// 5. 注册 Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgriTech IoT Platform API",
        Version = "v1",
        Description = "智能农业物联网设备管理平台 API",
        Contact = new OpenApiContact
        {
            Name = "AgriTech Team",
            Email = "support@agritech.com",
            Url = new Uri("https://github.com/your-repo")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.UseAllOfToExtendReferenceSchemas();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 6. 注册健康检查
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "postgres" });

// 7. 注册 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://agritech-frontend.azurewebsites.net"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// 8. 注册全局异常处理
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 9. 添加内存缓存
builder.Services.AddMemoryCache();

// ============ 10. 注册 MassTransit + RabbitMQ ============
builder.Services.AddMassTransit(x =>
{
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
            // 不配置消费者，只创建队列
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// ============ 中间件管道配置 ============

// ✅ 强制开启 Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AgriTech API v1");
    options.RoutePrefix = "swagger";
    options.DisplayRequestDuration();
    options.EnableTryItOutByDefault();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ============ 健康检查端点 ============
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration.TotalSeconds,
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration.TotalSeconds,
                Data = e.Value.Data
            }),
            Timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

// ============ 启动数据库 ============
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
        await SeedData.InitializeAsync(dbContext);

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database initialized and seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();