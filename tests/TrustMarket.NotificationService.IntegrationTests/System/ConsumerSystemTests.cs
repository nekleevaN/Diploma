using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.NotificationService.Application.Consumers;
using TrustMarket.NotificationService.IntegrationTests.Fakes;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.NotificationService.IntegrationTests.System;

public class ConsumerSystemTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    public FakeTelegramNotifier Notifier { get; } = new();

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ITelegramNotifier>(Notifier)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<UserRegisteredConsumer>();
                x.AddConsumer<UserEmailConfirmedConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Register_EmailConfirmed_CorrectNotificationsInOrder()
    {
        var userId = Guid.NewGuid();

        await _harness.Bus.Publish(new UserRegisteredIntegrationEvent(
            userId, "seq@example.com", "sequser", "Послідовний"));

        await _harness.Bus.Publish(new UserEmailConfirmedIntegrationEvent(
            userId, "seq@example.com", "Послідовний"));

        await _harness.Consumed.Any<UserRegisteredIntegrationEvent>();
        await _harness.Consumed.Any<UserEmailConfirmedIntegrationEvent>();

        Notifier.SentMessages.Should().HaveCountGreaterOrEqualTo(2);
        Notifier.SentMessages.Should().AllSatisfy(m => m.IsAdmin.Should().BeTrue());
    }
}
