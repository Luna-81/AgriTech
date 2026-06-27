using MediatR;
using Application.Common.Models;

namespace Application.Features.Farms.Commands.CreateFarm;

public class CreateFarmCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int MaxCapacity { get; set; }
}