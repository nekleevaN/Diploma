using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.FinanceService.Application.Webhooks;

public record ProcessMonobankWebhookCommand(MonobankInvoiceStatus Webhook) : IRequest;

public class ProcessMonobankWebhookCommandHandler : IRequestHandler<ProcessMonobankWebhookCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMonobankService _monobank;
    private readonly ILogger<ProcessMonobankWebhookCommandHandler> _logger;

    public ProcessMonobankWebhookCommandHandler(
        IOrderRepository orderRepository,
        IPublishEndpoint publishEndpoint,
        IMonobankService monobank,
        ILogger<ProcessMonobankWebhookCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
        _monobank = monobank;
        _logger = logger;
    }

    public async Task Handle(ProcessMonobankWebhookCommand request, CancellationToken ct)
    {
        var webhook = request.Webhook;

        if (!Guid.TryParse(webhook.Reference, out var orderId))
        {
            _logger.LogWarning("Невалідний Reference у webhook: {Reference}", webhook.Reference);
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} не знайдено для webhook {InvoiceId}", orderId, webhook.InvoiceId);
            return;
        }

        if (webhook.ModifiedDate.HasValue &&
            order.LastWebhookAt.HasValue &&
            webhook.ModifiedDate.Value <= order.LastWebhookAt.Value)
        {
            _logger.LogInformation("Webhook для Order {OrderId} застарілий, пропускаємо", orderId);
            return;
        }

        switch (webhook.Status)
        {
            case "hold":
                order.MarkAsPaid(webhook.ModifiedDate ?? DateTime.UtcNow);
                _logger.LogInformation("Order {OrderId}: HOLD — кошти заморожено (₴{Amount})", orderId, order.Amount);
                await _publishEndpoint.Publish(new OrderPaidIntegrationEvent(
                    order.Id, order.AdvertisementId, order.BuyerId, order.SellerId,
                    order.AdTitle, order.Amount), ct);

                if (!order.HasDelivery && !string.IsNullOrEmpty(order.InvoiceId))
                {
                    var finalized = await _monobank.FinalizeHoldAsync(order.InvoiceId, order.Amount, ct);
                    _logger.LogInformation("Order {OrderId}: auto-finalize (no delivery) — {Result}", orderId, finalized ? "ok" : "failed");
                }
                break;

            case "success":
                order.MarkAsCompleted(webhook.ModifiedDate ?? DateTime.UtcNow);
                _logger.LogInformation("Order {OrderId}: SUCCESS — кошти списано", orderId);
                await _publishEndpoint.Publish(new OrderCompletedIntegrationEvent(
                    order.Id, order.AdvertisementId, order.BuyerId, order.SellerId, order.Amount), ct);
                break;

            case "reversed":
                order.MarkAsRefunded(webhook.ModifiedDate ?? DateTime.UtcNow);
                _logger.LogInformation("Order {OrderId}: REVERSED — повернення коштів", orderId);
                await _publishEndpoint.Publish(new OrderCancelledIntegrationEvent(
                    order.Id, order.AdvertisementId, order.BuyerId, "Повернення коштів"), ct);
                break;

            case "failure":
                order.MarkAsFailed(webhook.FailureReason ?? webhook.ErrCode ?? "unknown");
                _logger.LogWarning("Order {OrderId}: FAILURE — {Reason}", orderId, webhook.FailureReason);
                break;

            case "expired":
                order.MarkAsExpired();
                _logger.LogInformation("Order {OrderId}: EXPIRED — час оплати вийшов", orderId);
                break;

            case "created":
            case "processing":
                break;
        }

        order.LastWebhookAt = webhook.ModifiedDate ?? DateTime.UtcNow;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);
    }
}
