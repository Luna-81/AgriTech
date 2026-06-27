using Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
   where TRequest : IRequest<TResponse>
    where TResponse : class
{
    
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).Select(f => f.ErrorMessage).ToList();

        if (failures.Any())
        {
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = responseType.GetMethod("Failure", new[] { typeof(List<string>) });
                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, new object[] { failures });
                    return (TResponse)(result ?? throw new InvalidOperationException("Result.Failure returned null"));
                }
            }
            throw new InvalidOperationException($"Type {responseType.Name} does not have a valid Failure method.");

        }
        return await next();
    
    
    }
}