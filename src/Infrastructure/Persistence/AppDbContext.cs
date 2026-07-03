using Domain.Common.Interfaces;
using Domain.Farms.Entities;
using Domain.Sensors.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Domain.Common;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppDbContext> _logger;
    private IDbContextTransaction? _currentTransaction;

    public AppDbContext(
        DbContextOptions<AppDbContext> options, 
        IMediator mediator, 
        ILogger<AppDbContext> logger) : base(options)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Sensor>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Farm>().HasQueryFilter(f => !f.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        await DispatchDomainEventsAsync();

        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        var events = domainEntities.SelectMany(x => x.Entity.DomainEvents!).ToList();
        domainEntities.ForEach(x => x.Entity.ClearDomainEvents());

        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent);
        }
    }


    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }


    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
       if (_currentTransaction == null) return;

       try
        {
            await SaveChangesAsync(cancellationToken); 
            await _currentTransaction!.CommitAsync(cancellationToken);
        }
        finally
        {
            _currentTransaction?.Dispose(); 
            _currentTransaction = null;
        }
    }


    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;
        try 
        { 
            await _currentTransaction.RollbackAsync(cancellationToken); 
        }
        finally 
        { 
            await _currentTransaction.DisposeAsync(); 
            _currentTransaction = null; 
        }
    }
}