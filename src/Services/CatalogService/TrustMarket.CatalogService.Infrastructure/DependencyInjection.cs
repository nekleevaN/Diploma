using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Infrastructure.Payment;
using TrustMarket.CatalogService.Infrastructure.Persistence;
using TrustMarket.CatalogService.Infrastructure.Messaging;
using TrustMarket.CatalogService.Infrastructure.Repositories;

namespace TrustMarket.CatalogService.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CatalogDb")));

        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();

        services.AddHttpClient<IMonobankService, MonobankService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPaidCatalogConsumer>();
            x.AddConsumer<OrderCompletedCatalogConsumer>();
            x.AddConsumer<OrderCancelledCatalogConsumer>();
            x.AddConsumer<UserProfileUpdatedConsumer>();
            x.AddConsumer<UserPayoutUpdatedConsumer>();

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
