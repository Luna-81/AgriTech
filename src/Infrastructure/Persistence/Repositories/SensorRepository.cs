using Domain.Common.Models;
using Domain.Sensors.Entities;
using Domain.Sensors.Enums;
using Domain.Sensors.RepositoryInterfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// 传感器仓储实现
/// </summary>
public class SensorRepository : EfRepository<Sensor>, ISensorRepository
{
    public SensorRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Sensor?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Name == name && !s.IsDeleted, cancellationToken);
    }

    // ✅ 修正：通过 Farm 查询传感器，而不是直接通过 FarmId
    public async Task<List<Sensor>> GetByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        // 通过 _context.Farms 查询，然后获取 Sensors
        var farm = await _context.Farms
            .Include(f => f.Sensors)
            .FirstOrDefaultAsync(f => f.Id == farmId && !f.IsDeleted, cancellationToken);

        return farm?.Sensors?.Where(s => !s.IsDeleted).ToList() ?? new List<Sensor>();
    }

    public async Task<List<Sensor>> GetActiveSensorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == SensorStatus.Active && !s.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sensor?> GetSensorWithReadingsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Readings)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(s => s.Name == name && !s.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Sensor>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? farmId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // ✅ 修正：通过 Farm 过滤
        if (farmId.HasValue)
        {
            // 先获取 Farm 的 Sensors
            var farm = await _context.Farms
                .Include(f => f.Sensors)
                .FirstOrDefaultAsync(f => f.Id == farmId.Value && !f.IsDeleted, cancellationToken);
            
            if (farm != null)
            {
                var sensorIds = farm.Sensors.Select(s => s.Id).ToList();
                query = query.Where(s => sensorIds.Contains(s.Id));
            }
            else
            {
                return new PagedResult<Sensor>(new List<Sensor>(), 0, page, pageSize);
            }
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => s.Name.Contains(searchTerm));
        }

        query = query.Where(s => !s.IsDeleted);
        
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Sensor>.Create(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<Reading>> GetReadingsPagedAsync(
        Guid sensorId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var sensor = await _dbSet
            .Include(s => s.Readings)
            .FirstOrDefaultAsync(s => s.Id == sensorId && !s.IsDeleted, cancellationToken);

        if (sensor == null)
        {
            return new PagedResult<Reading>(new List<Reading>(), 0, page, pageSize);
        }

        var readings = sensor.Readings.AsQueryable();

        if (startDate.HasValue)
        {
            readings = readings.Where(r => r.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            readings = readings.Where(r => r.Timestamp <= endDate.Value);
        }

        var totalCount = readings.Count();
        var items = readings
            .OrderByDescending(r => r.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<Reading>.Create(items, totalCount, page, pageSize);
    }
}