using Application.Common.Models;
using MediatR;

namespace Application.Features.Sensors.Commands.RegisterSensor;

public record RegisterSensorCommand: IRequest<Result<Guid>>
{
    public string Name{get; init;} = string.Empty;
    public double TemperatureThreshold{get; init;}
    public double Latitude{get; init;}
    public double Longitude{get; init;}
    public Guid FarmId{get; init;}
}