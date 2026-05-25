using FluentValidation;
using MediatR;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0) return await next();

        var error = string.Join("; ", failures.Select(f => f.ErrorMessage));

        var type = typeof(TResponse);

        if (type == typeof(Result))
            return (Result.Failure(error) as TResponse)!;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var method = typeof(Result)
                .GetMethods()
                .First(m => m.Name == "Failure" && m.IsGenericMethod)
                .MakeGenericMethod(type.GetGenericArguments()[0]);

            return (method.Invoke(null, [error]) as TResponse)!;
        }

        throw new ValidationException(failures);
    }
}
