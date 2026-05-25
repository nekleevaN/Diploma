using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.NotificationService.Application.Consumers;
using TrustMarket.NotificationService.Infrastructure.Telegram;

namespace TrustMarket.NotificationService.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var botToken = configuration["Telegram:BotToken"]
            ?? throw new InvalidOperationException("Telegram:BotToken не налаштовано");

        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
        services.AddScoped<ITelegramNotifier, TelegramNotifier>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<SuspiciousMessageConsumer>();
            x.AddConsumer<OrderPaidConsumer>();
            x.AddConsumer<OrderCompletedConsumer>();
            x.AddConsumer<ViewingConfirmedConsumer>();
            x.AddConsumer<ReviewReminderConsumer>();
            x.AddConsumer<UserEmailConfirmedConsumer>();
            x.AddConsumer<OfferRespondedConsumer>();

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
