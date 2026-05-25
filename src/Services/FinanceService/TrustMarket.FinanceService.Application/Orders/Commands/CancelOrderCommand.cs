using MassTransit;
using MediatR;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.FinanceService.Application.Orders.Commands;

public record CancelOrderCommand(Guid OrderId, Guid RequesterId) : IRequest<Result>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMonobankService _monobank;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IMonobankService monobank,
        IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _monobank = monobank;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure("Замовлення не знайдено");

        if (order.BuyerId != request.RequesterId && order.SellerId != request.RequesterId)
            return Result.Failure("Доступ заборонено");

        if (order.Status is not (OrderStatus.Pending or OrderStatus.Hold))
            return Result.Failure($"Неможливо скасувати: статус '{order.Status}'");

        if (!string.IsNullOrEmpty(order.InvoiceId))
        {
            await _monobank.CancelInvoiceAsync(order.InvoiceId, ct);
        }

        order.MarkAsCancelled();
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OrderCancelledIntegrationEvent(
            order.Id, order.AdvertisementId, order.BuyerId, "Скасовано"), ct);

        return Result.Success();
    }
}
