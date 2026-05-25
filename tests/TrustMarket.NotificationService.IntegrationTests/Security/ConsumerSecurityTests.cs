using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.NotificationService.Application.Consumers;
using TrustMarket.NotificationService.IntegrationTests.Fakes;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.NotificationService.IntegrationTests.Security;

public class ConsumerSecurityTests : IAsyncLifetime
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
    public async Task UserRegisteredEvent_EmptyFields_ConsumerHandlesGracefully()
    {
        // Consumer should not throw on edge-case input
        var evt = new UserRegisteredIntegrationEvent(
            Guid.NewGuid(), "", "", "");

        var act = async () =>
        {
            await _harness.Bus.Publish(evt);
            await _harness.Consumed.Any<UserRegisteredIntegrationEvent>();
        };

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UserEmailConfirmedEvent_SpecialCharsInName_ConsumerHandlesGracefully()
    {
        var evt = new UserEmailConfirmedIntegrationEvent(
            Guid.NewGuid(), "test@example.com", "<script>alert('xss')</script>");

        var act = async () =>
        {
            await _harness.Bus.Publish(evt);
            await _harness.Consumed.Any<UserEmailConfirmedIntegrationEvent>();
        };

        await act.Should().NotThrowAsync();
        // Notifier received message — consumer didn't crash
        Notifier.SentMessages.Should().NotBeEmpty();
    }
}
