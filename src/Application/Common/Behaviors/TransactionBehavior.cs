using Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!typeof(TRequest).Name.EndsWith("Command"))
            return await next();
        
        _logger.LogInformation("Starting transaction for {RequestName}", typeof(TRequest).Name);

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var response = await next();
            await _unitOfWork.CommitTransactionAsync(ct);
            _logger.LogInformation("Transaction committed for {RequestName}", typeof(TRequest).Name);
            return response;
            
        }
        catch(Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Transaction rolled back for {RequestName}", typeof(TRequest).Name);
            throw;
        }

    }
    
}