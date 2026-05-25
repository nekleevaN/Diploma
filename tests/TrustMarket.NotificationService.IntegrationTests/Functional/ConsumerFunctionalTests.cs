using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.NotificationService.Application.Consumers;
using TrustMarket.NotificationService.IntegrationTests.Fakes;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.NotificationService.IntegrationTests.Functional;

public class ConsumerFunctionalTests : IAsyncLifetime
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
                x.AddConsumer<SuspiciousMessageConsumer>();
                x.AddConsumer<OrderPaidConsumer>();
                x.AddConsumer<OrderCompletedConsumer>();
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
    public async Task UserEmailConfirmedEvent_NotificationContainsFirstName()
    {
        var evt = new UserEmailConfirmedIntegrationEvent(
            Guid.NewGuid(), "maria@example.com", "Марія");

        await _harness.Bus.Publish(evt);
        await _harness.Consumed.Any<UserEmailConfirmedIntegrationEvent>();

        Notifier.SentMessages.Should().ContainSingle(m =>
            m.Message.Contains("Марія") && m.IsAdmin);
    }

    [Fact]
    public async Task SuspiciousMessageEvent_AdminNotified()
    {
        var evt = new SuspiciousMessageDetectedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Phone number detected", 80, DateTime.UtcNow);

        await _harness.Bus.Publish(evt);
        (await _harness.Consumed.Any<SuspiciousMessageDetectedIntegrationEvent>())
            .Should().BeTrue();
    }

    [Fact]
    public async Task OrderPaidEvent_ConsumerHandlesGracefully()
    {
        var evt = new OrderPaidIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Ноутбук", 15000m);

        await _harness.Bus.Publish(evt);
        (await _harness.Consumed.Any<OrderPaidIntegrationEvent>())
            .Should().BeTrue();
    }
}
