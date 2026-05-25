using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.NotificationService.Application.Consumers;
using TrustMarket.NotificationService.IntegrationTests.Fakes;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.NotificationService.IntegrationTests.Integration;

public class ConsumerIntegrationTests : IAsyncLifetime
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
    public async Task UserRegisteredEvent_ConsumerSendsAdminNotification()
    {
        var evt = new UserRegisteredIntegrationEvent(
            Guid.NewGuid(), "user@example.com", "testuser", "Тест");

        await _harness.Bus.Publish(evt);

        (await _harness.Consumed.Any<UserRegisteredIntegrationEvent>())
            .Should().BeTrue();

        Notifier.SentMessages.Should().ContainSingle(m =>
            m.IsAdmin && m.Message.Contains("user@example.com"));
    }

    [Fact]
    public async Task UserEmailConfirmedEvent_ConsumerSendsAdminNotification()
    {
        var evt = new UserEmailConfirmedIntegrationEvent(
            Guid.NewGuid(), "confirmed@example.com", "Тест");

        await _harness.Bus.Publish(evt);

        (await _harness.Consumed.Any<UserEmailConfirmedIntegrationEvent>())
            .Should().BeTrue();

        Notifier.SentMessages.Should().ContainSingle(m =>
            m.IsAdmin && m.Message.Contains("confirmed@example.com"));
    }
}
