using MediatR;
using Application.Common.Models;

namespace Application.Features.Sensors.Commands.DeactivateSensor;

public class DeactivateSensorCommand : IRequest<Result<bool>>
{
    public Guid SensorId { get; set; }
}