using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrustMarket.TestInfrastructure.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.GoogleAuth;
using TrustMarket.UserService.Infrastructure.Persistence;
using TrustMarket.UserService.IntegrationTests.Fakes;
using Xunit;

namespace TrustMarket.UserService.IntegrationTests.Factories;

public class UserServiceWebAppFactory : BaseWebAppFactory<Program>
{
    public FakeEmailSender EmailSender { get; } = new();
    public FakeGoogleTokenValidator GoogleValidator { get; } = new();
    public FakeDiiaService DiiaService { get; } = new();

    public UserServiceWebAppFactory(PostgresContainerFixture _) { }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        UseSqliteFor<UserDbContext>(services);

        services.RemoveAll<IEmailSender>();
        services.RemoveAll<IGoogleTokenValidator>();
        services.RemoveAll<IDiiaService>();

        services.AddSingleton<IEmailSender>(EmailSender);
        services.AddSingleton<IGoogleTokenValidator>(GoogleValidator);
        services.AddSingleton<IDiiaService>(DiiaService);
    }
}

[CollectionDefinition("UserService")]
public class UserServiceCollection : ICollectionFixture<PostgresContainerFixture> { }
