using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Infrastructure.Persistence;
using TrustMarket.CatalogService.IntegrationTests.Fakes;
using TrustMarket.TestInfrastructure.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.CatalogService.IntegrationTests.Factories;

public class CatalogServiceWebAppFactory : BaseWebAppFactory<Program>
{
    public FakeCatalogMonobankService MonobankService { get; } = new();

    public CatalogServiceWebAppFactory(PostgresContainerFixture _) { }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        UseSqliteFor<CatalogDbContext>(services);
        services.RemoveAll<IMonobankService>();
        services.AddSingleton<IMonobankService>(MonobankService);
    }
}

[CollectionDefinition("CatalogService")]
public class CatalogServiceCollection : ICollectionFixture<PostgresContainerFixture> { }
