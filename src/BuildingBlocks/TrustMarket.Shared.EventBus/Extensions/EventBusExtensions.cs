using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TrustMarket.Shared.EventBus.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddTrustMarketEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMq");
                var host = rabbitConfig["Host"] ?? "localhost";
                var username = rabbitConfig["Username"] ?? "guest";
                var password = rabbitConfig["Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
