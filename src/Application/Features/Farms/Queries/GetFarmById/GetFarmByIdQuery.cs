using MediatR;
using Application.Common.Models;
using Application.Features.Farms.DTOs;

namespace Application.Features.Farms.Queries.GetFarmById;

public class GetFarmByIdQuery : IRequest<Result<FarmDto>>
{
    public Guid Id { get; set; }
}