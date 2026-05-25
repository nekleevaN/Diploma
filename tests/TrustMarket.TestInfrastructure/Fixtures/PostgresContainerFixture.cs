using Xunit;

namespace TrustMarket.TestInfrastructure.Fixtures;

public class PostgresContainerFixture : IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}
