using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.FinanceService.Infrastructure.Background;
using TrustMarket.FinanceService.Infrastructure.CatalogService;
using TrustMarket.FinanceService.Infrastructure.Monobank;
using TrustMarket.FinanceService.Infrastructure.NovaPoshta;
using TrustMarket.FinanceService.Infrastructure.Persistence;
using TrustMarket.FinanceService.Infrastructure.Repositories;

namespace TrustMarket.FinanceService.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FinanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("FinanceDb")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();

        services.AddHostedService<DeliverySimulatorService>();
        services.AddHostedService<PendingOrderExpiryService>();

        services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["CatalogService:BaseUrl"] ?? "http://catalog-service:8080/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddHttpClient<INovaPoshtaService, NovaPoshtaService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IMonobankService, MonobankService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "TrustMarket/1.0");
        });

        services.AddMassTransit(x =>
        {
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
