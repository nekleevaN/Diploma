using MassTransit;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.ChatService.Infrastructure.Messaging;

public class SuspiciousMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public SuspiciousMessagePublisher(IPublishEndpoint publishEndpoint)
        => _publishEndpoint = publishEndpoint;

    public Task PublishAsync(SuspiciousMessageDetectedIntegrationEvent @event, CancellationToken ct = default)
        => _publishEndpoint.Publish(@event, ct);
}
