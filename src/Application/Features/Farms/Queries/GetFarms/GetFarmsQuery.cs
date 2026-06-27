using MediatR;
using Application.Common.Models;
using Application.Features.Farms.DTOs;
using Domain.Common.Models;

namespace Application.Features.Farms.Queries.GetFarms;

public class GetFarmsQuery : IRequest<Result<PagedResult<FarmDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}