using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly ITelegramNotifier _notifier;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ITelegramNotifier notifier, ILogger<UserRegisteredConsumer> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Новий користувач: {Email}", evt.Email);

        var message = $"👤 *Новий користувач*\n" +
                      $"Ім'я: {evt.FirstName}\n" +
                      $"Username: {evt.Username}\n" +
                      $"Email: {evt.Email}\n" +
                      $"ID: `{evt.UserId}`\n" +
                      $"Час: {evt.OccurredOn:dd.MM.yyyy HH:mm} UTC";

        await _notifier.SendToAdminAsync(message, context.CancellationToken);
    }
}
