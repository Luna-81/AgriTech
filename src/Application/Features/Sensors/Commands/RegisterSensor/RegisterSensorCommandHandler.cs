using Application.Common.Models;
using Domain.Common.Interfaces;
using Domain.Sensors.Entities;
using Domain.Farms.Entities;
using Domain.Sensors.ValueObjects;
using MediatR;
using Domain.Common.Exceptions;
using Domain.Shared.ValueObjects;


namespace Application.Features.Sensors.Commands.RegisterSensor;

public class RegisterSensorCommandHandler : IRequestHandler<RegisterSensorCommand, Result<Guid>>
{
    private readonly IRepository<Farm> _farmRepository;
    private readonly IRepository<Sensor> _sensorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterSensorCommandHandler(IRepository<Farm> farmRepository,IRepository<Sensor> sensorRepository,IUnitOfWork unitOfWork)
    {
        _farmRepository = farmRepository;
        _sensorRepository = sensorRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<Result<Guid>> Handle(RegisterSensorCommand request, CancellationToken cancellationToken)
    {
        // 1 Obtain the aggregate root Farm
        var farm = await _farmRepository.GetByIdAsync(request.FarmId,cancellationToken);
        if(farm == null) return Result<Guid>.Failure($"The farm with ID {request.FarmId} was not found.");

        try
        {
            // 2. Create entities (Business rule validation is performed in the Domain layer)
            var temperature = Temperature.FromCelsius(request.TemperatureThreshold);
            var location = Location.FromCoordinates(request.Latitude,request.Longitude);
            var sensor = Sensor.Create(request.Name, temperature,location);

            // 3. Perform aggregation operation (call the Farm's business method)
            // This will automatically throw a DomainException (if capacity exceeds limits, etc.), which will be intercepted by the upper-level global handler.
            farm.AddSensor(sensor);

            // 4. Persisting Data
            await _sensorRepository.AddAsync(sensor,cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(sensor.Id);
            
        }
        catch(DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch(Exception)
        {
            return Result<Guid>.Failure("A system error occurred during sensor registration.");
        }




    }

}