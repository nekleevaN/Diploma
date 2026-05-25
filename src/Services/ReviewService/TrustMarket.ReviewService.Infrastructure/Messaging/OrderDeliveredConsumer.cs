using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.ReviewService.Infrastructure.Messaging;

public class OrderDeliveredConsumer : IConsumer<OrderDeliveredIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderDeliveredConsumer> _logger;

    public OrderDeliveredConsumer(IMediator mediator, ILogger<OrderDeliveredConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderDeliveredIntegrationEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "OrderDelivered: OrderId={OrderId}, BuyerId={BuyerId}, SellerId={SellerId}",
            evt.OrderId, evt.BuyerId, evt.SellerId);

        var result = await _mediator.Send(new CreateReviewPlaceholdersCommand(
            evt.OrderId, evt.BuyerId, evt.SellerId,
            evt.BuyerName, evt.SellerName));

        if (result.IsFailure)
            _logger.LogWarning("Не вдалося створити placeholder-відгуки для Order {OrderId}: {Error}",
                evt.OrderId, result.Error);
    }
}
