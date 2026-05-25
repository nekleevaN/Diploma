using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class OrderCompletedConsumer : IConsumer<OrderCompletedIntegrationEvent>
{
    private readonly ITelegramNotifier _notifier;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(ITelegramNotifier notifier, ILogger<OrderCompletedConsumer> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Замовлення завершено: {OrderId}", evt.OrderId);

        var message = $"✅ *Угода завершена!*\n" +
                      $"Сума ₴{evt.Amount:N0} успішно зарахована продавцю.\n" +
                      $"ID замовлення: `{evt.OrderId}`";

        await _notifier.SendToAdminAsync(message, context.CancellationToken);
    }
}
