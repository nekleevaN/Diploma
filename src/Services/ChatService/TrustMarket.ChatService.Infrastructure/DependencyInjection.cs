using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Infrastructure.Background;
using TrustMarket.ChatService.Infrastructure.Messaging;
using TrustMarket.ChatService.Infrastructure.Persistence;
using TrustMarket.ChatService.Infrastructure.Repositories;

namespace TrustMarket.ChatService.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ChatDb")));

        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IViewingRequestRepository, ViewingRequestRepository>();
        services.AddScoped<SuspiciousMessagePublisher>();
        services.AddHostedService<ViewingFollowUpService>();

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
