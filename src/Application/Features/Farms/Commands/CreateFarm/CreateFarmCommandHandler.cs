using MediatR;
using Application.Common.Models;
using Domain.Farms.Entities;
using Domain.Farms.RepositoryInterfaces;
using Domain.Shared.ValueObjects;
using Domain.Common.Interfaces;

namespace Application.Features.Farms.Commands.CreateFarm;

public class CreateFarmCommandHandler : IRequestHandler<CreateFarmCommand, Result<Guid>>
{
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFarmCommandHandler(IFarmRepository farmRepository, IUnitOfWork unitOfWork)
    {
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateFarmCommand request, CancellationToken cancellationToken)
    {
        var location = Location.FromCoordinates(request.Latitude, request.Longitude);
        var farm = Farm.Create(request.Name, location, request.MaxCapacity);

        await _farmRepository.AddAsync(farm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(farm.Id);
    }
}