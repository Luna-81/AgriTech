using MediatR;
using Application.Common.Models;
using Domain.Sensors.RepositoryInterfaces;
using Domain.Common.Interfaces;

namespace Application.Features.Sensors.Commands.ActivateSensor;

public class ActivateSensorCommandHandler : IRequestHandler<ActivateSensorCommand, Result<bool>>
{
    private readonly ISensorRepository _sensorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateSensorCommandHandler(ISensorRepository sensorRepository, IUnitOfWork unitOfWork)
    {
        _sensorRepository = sensorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ActivateSensorCommand request, CancellationToken cancellationToken)
    {
        var sensor = await _sensorRepository.GetByIdAsync(request.SensorId, cancellationToken);
        if (sensor == null)
            return Result<bool>.Failure($"Sensor with ID '{request.SensorId}' was not found.");

        sensor.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}