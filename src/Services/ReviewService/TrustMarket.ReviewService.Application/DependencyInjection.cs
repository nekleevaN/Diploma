using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.ReviewService.Application.Reviews.Commands;

namespace TrustMarket.ReviewService.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(
            typeof(ApplicationServiceCollectionExtensions).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
