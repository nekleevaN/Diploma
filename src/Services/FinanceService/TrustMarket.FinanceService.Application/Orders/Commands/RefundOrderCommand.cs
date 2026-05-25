using MassTransit;
using MediatR;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.FinanceService.Application.Orders.Commands;

public record RefundOrderCommand(Guid OrderId, Guid BuyerId) : IRequest<Result>;

public class RefundOrderCommandHandler : IRequestHandler<RefundOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMonobankService _monobank;
    private readonly IPublishEndpoint _publishEndpoint;

    public RefundOrderCommandHandler(
        IOrderRepository orderRepository,
        IMonobankService monobank,
        IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _monobank = monobank;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(RefundOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure("Замовлення не знайдено");

        if (order.BuyerId != request.BuyerId)
            return Result.Failure("Доступ заборонено");

        if (order.Status != OrderStatus.AwaitingConfirmation)
            return Result.Failure($"Неможливо відшкодувати: статус замовлення '{order.Status}'");

        if (!string.IsNullOrEmpty(order.InvoiceId))
            await _monobank.CancelInvoiceAsync(order.InvoiceId, ct);

        order.MarkAsRefunded(DateTime.UtcNow);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OrderCancelledIntegrationEvent(
            order.Id, order.AdvertisementId, order.BuyerId, "Покупець запросив відшкодування"), ct);

        return Result.Success();
    }
}
