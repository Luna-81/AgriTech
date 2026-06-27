using MediatR;
using Application.Common.Models;
using Application.Features.Farms.DTOs;
using Domain.Common.Models;
using Domain.Farms.RepositoryInterfaces;

namespace Application.Features.Farms.Queries.GetFarms;

public class GetFarmsQueryHandler : IRequestHandler<GetFarmsQuery, Result<PagedResult<FarmDto>>>
{
    private readonly IFarmRepository _farmRepository;

    public GetFarmsQueryHandler(IFarmRepository farmRepository)
    {
        _farmRepository = farmRepository;
    }

    public async Task<Result<PagedResult<FarmDto>>> Handle(GetFarmsQuery request, CancellationToken cancellationToken)
    {
        var farms = await _farmRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            cancellationToken);

        var dtos = farms.Items.Select(f => new FarmDto
        {
            Id = f.Id,
            Name = f.Name,
            Latitude = f.Location.Latitude,
            Longitude = f.Location.Longitude,
            SensorCount = f.Sensors?.Count ?? 0,
            MaxCapacity = f.MaxSensorCapacity,
            CreatedAt = f.CreatedAt ?? DateTime.UtcNow
        }).ToList();

        var result = PagedResult<FarmDto>.Create(dtos, farms.TotalCount, request.Page, request.PageSize);
        return Result<PagedResult<FarmDto>>.Success(result);
    }
}