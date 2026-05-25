using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.ReviewService.Infrastructure.Background;
using TrustMarket.ReviewService.Infrastructure.Messaging;
using TrustMarket.ReviewService.Infrastructure.Persistence;
using TrustMarket.ReviewService.Infrastructure.Repositories;

namespace TrustMarket.ReviewService.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReviewDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ReviewDb")));

        services.AddScoped<IReviewRepository, ReviewRepository>();

        services.AddHostedService<ReviewMaintenanceService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderDeliveredConsumer>();

            if (configuration["MassTransit:UseInMemory"] == "true")
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            else
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RabbitMq:Username"] ?? "guest");
                        h.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(ctx);
                });
        });

        return services;
    }
}
