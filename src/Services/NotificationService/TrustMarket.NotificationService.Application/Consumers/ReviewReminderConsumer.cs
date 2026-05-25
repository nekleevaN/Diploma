using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class ReviewReminderConsumer : IConsumer<ReviewReminderIntegrationEvent>
{
    private readonly ITelegramNotifier _telegram;
    private readonly ILogger<ReviewReminderConsumer> _logger;

    public ReviewReminderConsumer(ITelegramNotifier telegram, ILogger<ReviewReminderConsumer> logger)
    {
        _telegram = telegram;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReviewReminderIntegrationEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Review reminder: ReviewId={ReviewId}, ReviewerId={ReviewerId}",
            evt.ReviewId, evt.ReviewerId);

        var role = evt.ReviewType == "BuyerToSeller" ? "продавця" : "покупця";
        var msg = $"⭐ *Нагадування про відгук*\n\n" +
                  $"Залиш відгук про {role} за замовленням «{evt.AdTitle}».\n" +
                  $"У тебе залишилось 11 днів.";

        await _telegram.SendToAdminAsync(msg, context.CancellationToken);
    }
}
