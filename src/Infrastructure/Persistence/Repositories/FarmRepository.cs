using Domain.Common.Models;
using Domain.Farms.Entities;
using Domain.Farms.RepositoryInterfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// 农场仓储实现
/// </summary>
public class FarmRepository : EfRepository<Farm>, IFarmRepository
{
    public FarmRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Farm?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.Name == name && !f.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(f => f.Name == name && !f.IsDeleted, cancellationToken);
    }

    public async Task<Farm?> GetFarmWithSensorsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Sensors)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Farm>> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(f => f.Name.Contains(searchTerm));
        }

        query = query.Where(f => !f.IsDeleted);
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Farm>.Create(items, totalCount, page, pageSize);
    }
}