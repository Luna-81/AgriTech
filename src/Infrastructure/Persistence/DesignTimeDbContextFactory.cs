using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using Infrastructure.Persistence;


namespace Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();


        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Host=localhost;Port=5432;Database=AgriTechDb;Username=postgres;Password=postgres;";

        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            npgsqlOptions.EnableRetryOnFailure(3);
        });

        return new AppDbContext(optionsBuilder.Options, null!, null!);

    }

}