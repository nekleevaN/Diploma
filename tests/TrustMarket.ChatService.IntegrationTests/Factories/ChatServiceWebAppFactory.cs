using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrustMarket.ChatService.Infrastructure.Background;
using TrustMarket.ChatService.Infrastructure.Persistence;
using TrustMarket.TestInfrastructure.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.ChatService.IntegrationTests.Factories;

public class ChatServiceWebAppFactory : BaseWebAppFactory<Program>
{
    public ChatServiceWebAppFactory(PostgresContainerFixture _) { }

    protected override void RemoveBackgroundServices(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IHostedService) &&
            d.ImplementationType == typeof(ViewingFollowUpService));
        if (descriptor != null)
            services.Remove(descriptor);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
        => UseSqliteFor<ChatDbContext>(services);
}

[CollectionDefinition("ChatService")]
public class ChatServiceCollection : ICollectionFixture<PostgresContainerFixture> { }
