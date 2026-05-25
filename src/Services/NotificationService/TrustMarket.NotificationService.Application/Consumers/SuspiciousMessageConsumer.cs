using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.NotificationService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.NotificationService.Application.Consumers;

public class SuspiciousMessageConsumer : IConsumer<SuspiciousMessageDetectedIntegrationEvent>
{
    private readonly ITelegramNotifier _notifier;
    private readonly ILogger<SuspiciousMessageConsumer> _logger;

    public SuspiciousMessageConsumer(ITelegramNotifier notifier, ILogger<SuspiciousMessageConsumer> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SuspiciousMessageDetectedIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogWarning(
            "Антифрод: score={Score}, sender={SenderId}, chat={ChatId}",
            evt.FraudScore, evt.SenderId, evt.ChatId);
        return Task.CompletedTask;
    }
}
