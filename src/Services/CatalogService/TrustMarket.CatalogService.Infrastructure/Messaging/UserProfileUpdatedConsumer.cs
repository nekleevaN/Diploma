using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.CatalogService.Infrastructure.Messaging;

public class UserProfileUpdatedConsumer : IConsumer<UserProfileUpdatedIntegrationEvent>
{
    private readonly IAdvertisementRepository _repo;
    private readonly ILogger<UserProfileUpdatedConsumer> _logger;

    public UserProfileUpdatedConsumer(IAdvertisementRepository repo, ILogger<UserProfileUpdatedConsumer> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserProfileUpdatedIntegrationEvent> context)
    {
        var evt = context.Message;

        await _repo.UpdateSellerNameAsync(evt.UserId, evt.DisplayName, context.CancellationToken);

        _logger.LogInformation(
            "SellerName оновлено для UserId={UserId}: '{DisplayName}'",
            evt.UserId, evt.DisplayName);
    }
}
