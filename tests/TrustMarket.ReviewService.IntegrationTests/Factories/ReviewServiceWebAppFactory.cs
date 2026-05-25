using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrustMarket.ReviewService.Infrastructure.Background;
using TrustMarket.ReviewService.Infrastructure.Persistence;
using TrustMarket.TestInfrastructure.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.ReviewService.IntegrationTests.Factories;

public class ReviewServiceWebAppFactory : BaseWebAppFactory<Program>
{
    public ReviewServiceWebAppFactory(PostgresContainerFixture _) { }

    protected override void RemoveBackgroundServices(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IHostedService) &&
            d.ImplementationType == typeof(ReviewMaintenanceService));
        if (descriptor != null)
            services.Remove(descriptor);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
        => UseSqliteFor<ReviewDbContext>(services);
}

[CollectionDefinition("ReviewService")]
public class ReviewServiceCollection : ICollectionFixture<PostgresContainerFixture> { }
