using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Infrastructure.Persistence;

namespace TrustMarket.FinanceService.Infrastructure.Background;

public class PendingOrderExpiryService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ExpiryThreshold = TimeSpan.FromMinutes(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingOrderExpiryService> _logger;

    public PendingOrderExpiryService(IServiceScopeFactory scopeFactory, ILogger<PendingOrderExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервіс закінчення терміну очікування замовлень запущено");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                await ExpireStaleOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час закінчення терміну очікування замовлень");
            }
        }
    }

    private async Task ExpireStaleOrdersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var catalog = scope.ServiceProvider.GetRequiredService<ICatalogServiceClient>();

        var cutoff = DateTime.UtcNow - ExpiryThreshold;

        var staleOrders = await db.Orders
            .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < cutoff)
            .ToListAsync(ct);

        if (staleOrders.Count == 0)
            return;

        _logger.LogInformation("Знайдено {Count} прострочених Pending замовлень", staleOrders.Count);

        foreach (var order in staleOrders)
        {
            order.MarkAsExpired();

            try
            {
                await catalog.UnreserveAdvertisementAsync(order.AdvertisementId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не вдалося зняти резервацію оголошення {AdId} для замовлення {OrderId}",
                    order.AdvertisementId, order.Id);
            }

            _logger.LogInformation("Замовлення {OrderId} (оголошення {AdId}) закінчило термін дії",
                order.Id, order.AdvertisementId);
        }

        await db.SaveChangesAsync(ct);
    }
}
