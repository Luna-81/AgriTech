using MediatR;
using Application.Common.Models;
using Application.Features.Farms.DTOs;
using Domain.Farms.RepositoryInterfaces;

namespace Application.Features.Farms.Queries.GetFarmById;

public class GetFarmByIdQueryHandler : IRequestHandler<GetFarmByIdQuery, Result<FarmDto>>
{
    private readonly IFarmRepository _farmRepository;

    public GetFarmByIdQueryHandler(IFarmRepository farmRepository)
    {
        _farmRepository = farmRepository;
    }

    public async Task<Result<FarmDto>> Handle(GetFarmByIdQuery request, CancellationToken cancellationToken)
    {
        var farm = await _farmRepository.GetByIdAsync(request.Id, cancellationToken);

        if (farm == null)
            return Result<FarmDto>.Failure($"农场 '{request.Id}' 不存在。");

        var dto = new FarmDto
        {
            Id = farm.Id,
            Name = farm.Name,
            Latitude = farm.Location.Latitude,
            Longitude = farm.Location.Longitude,
            SensorCount = farm.Sensors?.Count ?? 0,
            MaxCapacity = farm.MaxSensorCapacity,
            CreatedAt = farm.CreatedAt ?? DateTime.UtcNow
        };

        return Result<FarmDto>.Success(dto);
    }
}