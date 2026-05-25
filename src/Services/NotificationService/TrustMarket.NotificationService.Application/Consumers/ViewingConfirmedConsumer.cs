using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class ViewingConfirmedConsumer : IConsumer<ViewingConfirmedIntegrationEvent>
{
    private readonly ITelegramNotifier _notifier;
    private readonly ILogger<ViewingConfirmedConsumer> _logger;

    public ViewingConfirmedConsumer(ITelegramNotifier notifier, ILogger<ViewingConfirmedConsumer> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ViewingConfirmedIntegrationEvent> context)
    {
        var evt = context.Message;
        var dt = evt.ViewingDateTime.ToLocalTime();
        var ct = context.CancellationToken;

        _logger.LogInformation("Перегляд підтверджено: {ViewingId}, {AdTitle}, {DateTime}",
            evt.ViewingId, evt.AdTitle, dt);

        if (evt.BuyerTrustedTelegramId is > 0)
        {
            var location = evt.LocationAddress != null ? $"\n📍 {evt.LocationAddress}" : "";
            var msgForBuyerContact =
                $"🛡️ *Сповіщення безпеки TrustMarket*\n\n" +
                $"👤 *Покупець:* {evt.BuyerName}\n" +
                $"🏷️ *Продавець:* {evt.SellerName}\n\n" +
                $"📦 {evt.AdTitle}\n" +
                $"🕐 *{dt:dd.MM.yyyy} о {dt:HH:mm}*" +
                $"{location}\n\n" +
                $"_{evt.BuyerName} іде на перегляд до {evt.SellerName}._\n" +
                $"_Якщо не повернеться вчасно — зв'яжіться з ним або зверніться до поліції._";

            await _notifier.SendToUserAsync(evt.BuyerTrustedTelegramId.Value, msgForBuyerContact, ct);
            _logger.LogInformation("Telegram надіслано довіреній особі покупця (ID:{Id})", evt.BuyerTrustedTelegramId);
        }

        if (evt.SellerTrustedTelegramId is > 0)
        {
            var location = evt.LocationAddress != null ? $"\n📍 {evt.LocationAddress}" : "";
            var msgForSellerContact =
                $"🛡️ *Сповіщення безпеки TrustMarket*\n\n" +
                $"👤 *Покупець:* {evt.BuyerName}\n" +
                $"🏷️ *Продавець:* {evt.SellerName}\n\n" +
                $"📦 {evt.AdTitle}\n" +
                $"🕐 *{dt:dd.MM.yyyy} о {dt:HH:mm}*" +
                $"{location}\n\n" +
                $"_{evt.BuyerName} іде на перегляд до {evt.SellerName}._\n" +
                $"_Це автоматичне сповіщення безпеки від TrustMarket._";

            await _notifier.SendToUserAsync(evt.SellerTrustedTelegramId.Value, msgForSellerContact, ct);
            _logger.LogInformation("Telegram надіслано довіреній особі продавця (ID:{Id})", evt.SellerTrustedTelegramId);
        }

        if (evt.BuyerTrustedTelegramId is not > 0 && evt.SellerTrustedTelegramId is not > 0)
        {
            await _notifier.SendToAdminAsync(
                $"ℹ️ Перегляд підтверджено, але жоден з учасників не вказав довірену особу.\n" +
                $"📦 {evt.AdTitle} | 🕐 {dt:dd.MM.yyyy HH:mm}", ct);
        }
    }
}
