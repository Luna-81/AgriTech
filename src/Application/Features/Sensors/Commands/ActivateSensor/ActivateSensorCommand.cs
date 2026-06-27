using MediatR;
using Application.Common.Models;

namespace Application.Features.Sensors.Commands.ActivateSensor;

public class ActivateSensorCommand : IRequest<Result<bool>>
{
    public Guid SensorId { get; set; }
}