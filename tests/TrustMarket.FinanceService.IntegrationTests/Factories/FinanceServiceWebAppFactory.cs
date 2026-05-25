using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Infrastructure.Background;
using TrustMarket.FinanceService.Infrastructure.Persistence;
using TrustMarket.FinanceService.IntegrationTests.Fakes;
using TrustMarket.TestInfrastructure.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.FinanceService.IntegrationTests.Factories;

public class FinanceServiceWebAppFactory : BaseWebAppFactory<Program>
{
    public FakeMonobankService MonobankService { get; } = new();
    public FakeCatalogServiceClient CatalogClient { get; } = new();
    public FakeNovaPoshtaService NovaPoshtaService { get; } = new();

    public FinanceServiceWebAppFactory(PostgresContainerFixture _) { }

    protected override void RemoveBackgroundServices(IServiceCollection services)
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService) &&
                        (d.ImplementationType == typeof(DeliverySimulatorService) ||
                         d.ImplementationType == typeof(PendingOrderExpiryService)))
            .ToList();
        foreach (var d in descriptors)
            services.Remove(d);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        UseSqliteFor<FinanceDbContext>(services);
        services.RemoveAll<IMonobankService>();
        services.RemoveAll<ICatalogServiceClient>();
        services.RemoveAll<INovaPoshtaService>();
        services.AddSingleton<IMonobankService>(MonobankService);
        services.AddSingleton<ICatalogServiceClient>(CatalogClient);
        services.AddSingleton<INovaPoshtaService>(NovaPoshtaService);
    }
}

[CollectionDefinition("FinanceService")]
public class FinanceServiceCollection : ICollectionFixture<PostgresContainerFixture> { }
