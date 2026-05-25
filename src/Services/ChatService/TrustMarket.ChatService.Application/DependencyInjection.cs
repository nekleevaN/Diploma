using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TrustMarket.ChatService.Application.FraudDetection;

namespace TrustMarket.ChatService.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped<IFraudAnalyzer, RuleBasedFraudAnalyzer>();
        return services;
    }
}
