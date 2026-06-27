using Application.Common.Models;
using Application.Features.Sensors.DTOs;
using MediatR;



namespace Application.Features.Sensors.Queries.GetSensorHistory;

public record GetSensorHistoryQuery : IRequest<Result<List<SensorReadingDto>>>
{
    public Guid SensorId { get; init; } 
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
    
