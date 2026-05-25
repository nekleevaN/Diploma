using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.GoogleAuth;
using TrustMarket.UserService.Domain.Repositories;
using TrustMarket.UserService.Infrastructure.Auth;
using TrustMarket.UserService.Infrastructure.Email;
using TrustMarket.UserService.Infrastructure.External.Diia;
using TrustMarket.UserService.Infrastructure.Messaging;
using TrustMarket.UserService.Infrastructure.Persistence;
using TrustMarket.UserService.Infrastructure.Repositories;

namespace TrustMarket.UserService.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("UserDb")));

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        var emailProvider = configuration["Email:Provider"] ?? "Console";
        if (emailProvider == "Smtp")
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        else
            services.AddScoped<IEmailSender, ConsoleEmailSender>();

        var diiaProvider = configuration["Diia:Provider"] ?? "Mock";
        if (diiaProvider == "Mock")
            services.AddScoped<IDiiaService, MockDiiaService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ReviewPublishedConsumer>();
            x.AddConsumer<ViewingEmailNotificationConsumer>();

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
