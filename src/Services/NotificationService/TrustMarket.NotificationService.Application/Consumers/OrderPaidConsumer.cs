using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class OrderPaidConsumer : IConsumer<OrderPaidIntegrationEvent>
{
    private readonly ITelegramNotifier _notifier;
    private readonly ILogger<OrderPaidConsumer> _logger;

    public OrderPaidConsumer(ITelegramNotifier notifier, ILogger<OrderPaidConsumer> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPaidIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Замовлення оплачено: {OrderId}", evt.OrderId);

        var message = $"💰 *Нове замовлення!*\n" +
                      $"Товар: {evt.AdTitle}\n" +
                      $"Сума: *₴{evt.Amount:N0}* (кошти заморожено)\n" +
                      $"ID замовлення: `{evt.OrderId}`\n\n" +
                      $"⚡ Відправте товар і підтвердіть доставку в особистому кабінеті — після цього кошти надійдуть на ваш рахунок.";

        await _notifier.SendToAdminAsync(message, context.CancellationToken);
    }
}
