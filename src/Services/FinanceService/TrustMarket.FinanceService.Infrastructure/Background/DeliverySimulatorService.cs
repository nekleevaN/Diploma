using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Infrastructure.Persistence;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.FinanceService.Infrastructure.Background;

public class DeliverySimulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeliverySimulatorService> _logger;

    private static readonly Dictionary<DeliveryStatus, (DeliveryStatus Next, int DelayMinutes)> StatusFlow = new()
    {
        [DeliveryStatus.TTNCreated]  = (DeliveryStatus.AtWarehouse,  1),
        [DeliveryStatus.AtWarehouse] = (DeliveryStatus.InTransit,    2),
        [DeliveryStatus.InTransit]   = (DeliveryStatus.Arrived,      3),
        [DeliveryStatus.Arrived]     = (DeliveryStatus.Received,     1),
    };

    private static readonly Dictionary<DeliveryStatus, string> StatusDescriptions = new()
    {
        [DeliveryStatus.AtWarehouse] = "Відправлення прийнято у відділенні НП",
        [DeliveryStatus.InTransit]   = "Відправлення в дорозі",
        [DeliveryStatus.Arrived]     = "Відправлення прибуло у відділення одержувача",
        [DeliveryStatus.Received]    = "Відправлення отримано одержувачем",
    };

    public DeliverySimulatorService(IServiceScopeFactory scopeFactory, ILogger<DeliverySimulatorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Симулятор доставки НП запущено");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            try
            {
                await ProcessDeliveriesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка симулятора доставки");
            }
        }
    }

    private async Task ProcessDeliveriesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var now = DateTime.UtcNow;

        var activeDeliveries = await db.Deliveries
            .Include(d => d.Order)
            .Where(d => d.TTN != null &&
                        d.Status != DeliveryStatus.Received &&
                        d.Status != DeliveryStatus.Returned &&
                        d.Status != DeliveryStatus.PendingAddress &&
                        d.Status != DeliveryStatus.AddressSet)
            .ToListAsync(ct);

        foreach (var delivery in activeDeliveries)
        {
            if (!StatusFlow.TryGetValue(delivery.Status, out var transition))
                continue;

            var lastChange = delivery.UpdatedAt ?? delivery.CreatedAt;
            var elapsed = now - lastChange;

            if (elapsed.TotalMinutes < transition.DelayMinutes)
                continue;

            var newStatus = transition.Next;
            var description = StatusDescriptions.GetValueOrDefault(newStatus, newStatus.ToString());

            delivery.UpdateTracking(GetStatusCode(newStatus), description);

            _logger.LogInformation(
                "Доставка {DeliveryId} (ТТН {TTN}): {OldStatus} → {NewStatus}",
                delivery.Id, delivery.TTN, delivery.Status, newStatus);

            if (newStatus == DeliveryStatus.Received && delivery.Order != null)
            {
                if (delivery.Order.Status == OrderStatus.Hold)
                    delivery.Order.MarkAsAwaitingConfirmation();

                await bus.Publish(new OrderDeliveredIntegrationEvent(
                    delivery.Order.Id,
                    delivery.Order.BuyerId,
                    delivery.Order.SellerId,
                    delivery.Order.AdTitle,
                    delivery.RecipientName ?? "Покупець",
                    delivery.SenderName    ?? "Продавець"), ct);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static string GetStatusCode(DeliveryStatus status) => status switch
    {
        DeliveryStatus.AtWarehouse => "5",
        DeliveryStatus.InTransit   => "6",
        DeliveryStatus.Arrived     => "7",
        DeliveryStatus.Received    => "9",
        _ => "0"
    };
}
