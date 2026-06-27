using FluentValidation;

namespace Application.Features.Sensors.Commands.RegisterSensor;

public class RegisterSensorCommandValidator : AbstractValidator<RegisterSensorCommand>
{
    public RegisterSensorCommandValidator()
    {
        RuleFor(x=>x.Name).NotEmpty().WithMessage("The name of the sensor cannot be left blank.");
        RuleFor(x=>x.TemperatureThreshold).InclusiveBetween(-50,80).WithMessage("Exceeding the temperature threshold. ");
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("Latitude is invalid.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("Longitude is invalid.");
        RuleFor(x => x.FarmId).NotEmpty().WithMessage("The farm ID cannot be left blank.");
    }

}
