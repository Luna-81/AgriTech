using Application.Common.Models;
using Domain.Common.Interfaces;
using Application.Features.Sensors.DTOs;
using Domain.Sensors.Entities;
using MediatR;

namespace Application.Features.Sensors.Queries.GetSensorHistory;

public class GetSensorHistoryQueryHandler : IRequestHandler<GetSensorHistoryQuery, Result<List<SensorReadingDto>>>
{
    private readonly IRepository<Sensor> _sensorRepository;
    public GetSensorHistoryQueryHandler(IRepository<Sensor> sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    public async Task<Result<List<SensorReadingDto>>> Handle(GetSensorHistoryQuery request, CancellationToken ct)
    {
        var sensor = await _sensorRepository.GetByIdAsync(request.SensorId, ct);
        if (sensor == null)
            return Result<List<SensorReadingDto>>.Failure($"The sensor with ID {request.SensorId} was not found.");
   
        var readings = sensor.Readings
            .Where (r => (!request.StartDate.HasValue || r.Timestamp >= request.StartDate) &&
                        (!request.EndDate.HasValue || r.Timestamp <= request.EndDate))
            .Skip((request.Page -1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new SensorReadingDto
            {
                Timestamp = r.Timestamp,
                Temperature = r.Temperature,
                Humidity = r.Humidity
            })
            .ToList();

        return Result<List<SensorReadingDto>>.Success(readings);
    }
}