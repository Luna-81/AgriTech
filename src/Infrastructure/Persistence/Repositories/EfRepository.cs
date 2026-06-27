using Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // FindAsync 适用于主键查询
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        // DDD 软删除逻辑：检查是否实现了审计接口
        if (entity is IAuditableEntity auditable)
        {
            // 注意：这里需要你的实体类里有一个 SoftDelete() 方法或者直接设置 IsDeleted
            // 建议在实体内部处理状态变化
            auditable.IsDeleted = true;
            _dbSet.Update(entity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public IQueryable<T> Query() => _dbSet.AsQueryable();
    

    public async Task<T?> GetWithIncludesAsync(
        Guid id, 
        Func<IQueryable<T>, IQueryable<T>> includeExpression, 
        CancellationToken cancellationToken = default)
    {
        // 应用 Include 表达式并查询
        var query = includeExpression(_dbSet);
        
        // 使用 EF.Property 处理泛型 ID 比较
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    

}