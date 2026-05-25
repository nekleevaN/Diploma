using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.CatalogService.Infrastructure.Messaging;

public class UserPayoutUpdatedConsumer : IConsumer<UserPayoutMethodUpdatedIntegrationEvent>
{
    private readonly IAdvertisementRepository _repo;
    private readonly ILogger<UserPayoutUpdatedConsumer> _logger;

    public UserPayoutUpdatedConsumer(IAdvertisementRepository repo, ILogger<UserPayoutUpdatedConsumer> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserPayoutMethodUpdatedIntegrationEvent> context)
    {
        var evt = context.Message;

        await _repo.UpdateSellerSubMerchantIdAsync(evt.UserId, evt.MonobankSubMerchantId, context.CancellationToken);

        _logger.LogInformation(
            "SellerSubMerchantId оновлено для UserId={UserId}: {Status}",
            evt.UserId, evt.MonobankSubMerchantId is null ? "видалено" : "встановлено");
    }
}
