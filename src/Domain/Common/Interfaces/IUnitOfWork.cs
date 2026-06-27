namespace Domain.Common.Interfaces;

public interface IUnitOfWork
{
    // Synchronize all changes to the database
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Transaction Management
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}