using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.CatalogService.Infrastructure.Messaging;

public class OrderPaidCatalogConsumer : IConsumer<OrderPaidIntegrationEvent>
{
    private readonly IAdvertisementRepository _repo;
    private readonly ILogger<OrderPaidCatalogConsumer> _logger;

    public OrderPaidCatalogConsumer(IAdvertisementRepository repo, ILogger<OrderPaidCatalogConsumer> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPaidIntegrationEvent> context)
    {
        var evt = context.Message;
        var ad = await _repo.GetByIdAsync(evt.AdvertisementId, context.CancellationToken);
        if (ad is null) return;

        ad.MarkAsReserved();
        _repo.Update(ad);
        await _repo.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Оголошення {AdId} зарезервовано (замовлення {OrderId})",
            evt.AdvertisementId, evt.OrderId);
    }
}

public class OrderCompletedCatalogConsumer : IConsumer<OrderCompletedIntegrationEvent>
{
    private readonly IAdvertisementRepository _repo;
    private readonly ILogger<OrderCompletedCatalogConsumer> _logger;

    public OrderCompletedCatalogConsumer(IAdvertisementRepository repo, ILogger<OrderCompletedCatalogConsumer> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        var evt = context.Message;
        var ad = await _repo.GetByIdAsync(evt.AdvertisementId, context.CancellationToken);
        if (ad is null) return;

        ad.MarkAsSold();
        _repo.Update(ad);
        await _repo.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Оголошення {AdId} продано (замовлення {OrderId})",
            evt.AdvertisementId, evt.OrderId);
    }
}

public class OrderCancelledCatalogConsumer : IConsumer<OrderCancelledIntegrationEvent>
{
    private readonly IAdvertisementRepository _repo;
    private readonly ILogger<OrderCancelledCatalogConsumer> _logger;

    public OrderCancelledCatalogConsumer(IAdvertisementRepository repo, ILogger<OrderCancelledCatalogConsumer> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledIntegrationEvent> context)
    {
        var evt = context.Message;
        var ad = await _repo.GetByIdAsync(evt.AdvertisementId, context.CancellationToken);
        if (ad is null || ad.Status == Domain.Entities.AdvertisementStatus.Sold) return;

        ad.MarkAsActive();
        _repo.Update(ad);
        await _repo.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Оголошення {AdId} повернуто в Active (замовлення {OrderId} скасовано)",
            evt.AdvertisementId, evt.OrderId);
    }
}
