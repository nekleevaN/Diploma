using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class UserEmailConfirmedConsumer : IConsumer<UserEmailConfirmedIntegrationEvent>
{
    private readonly ITelegramNotifier _notifier;
    private readonly ILogger<UserEmailConfirmedConsumer> _logger;

    public UserEmailConfirmedConsumer(
        ITelegramNotifier notifier, ILogger<UserEmailConfirmedConsumer> logger)
    {
        _notifier = notifier;
        _logger   = logger;
    }

    public async Task Consume(ConsumeContext<UserEmailConfirmedIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Email підтверджено: {Email}", evt.Email);

        var message = $"✅ *Email підтверджено*\n" +
                      $"Ім'я: {evt.FirstName}\n" +
                      $"Email: {evt.Email}\n" +
                      $"ID: `{evt.UserId}`";

        await _notifier.SendToAdminAsync(message, context.CancellationToken);
    }
}
