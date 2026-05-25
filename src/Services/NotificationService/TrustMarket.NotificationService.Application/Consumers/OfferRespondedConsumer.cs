using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class OfferRespondedConsumer : IConsumer<OfferRespondedIntegrationEvent>
{
    private readonly ITelegramNotifier _notifier;
    private readonly ILogger<OfferRespondedConsumer> _logger;

    public OfferRespondedConsumer(ITelegramNotifier notifier, ILogger<OfferRespondedConsumer> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OfferRespondedIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Відповідь на пропозицію: {OfferId}, статус={Status}", evt.OfferId, evt.Status);

        var message = evt.Status switch
        {
            "Accepted" =>
                $"✅ *Продавець прийняв вашу пропозицію!*\n" +
                $"📦 Товар: {evt.AdTitle}\n" +
                $"Перейдіть до оформлення замовлення.",

            "Rejected" =>
                $"❌ *Продавець відхилив вашу пропозицію*\n" +
                $"📦 Товар: {evt.AdTitle}" +
                (evt.Note is not null ? $"\n💬 Причина: {evt.Note}" : ""),

            "CounterOffered" =>
                $"🔄 *Продавець запропонував нову ціну*\n" +
                $"📦 Товар: {evt.AdTitle}\n" +
                $"💰 Нова ціна: *₴{evt.CounterPrice:N0}*" +
                (evt.Note is not null ? $"\n💬 {evt.Note}" : ""),

            _ => null
        };

        if (message is null)
        {
            _logger.LogWarning("Невідомий статус відповіді на пропозицію: {Status}", evt.Status);
            return;
        }

        await _notifier.SendToAdminAsync(message, context.CancellationToken);
    }
}
