using MassTransit;
using MediatR;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.FinanceService.Application.Orders.Commands;

public record FinalizeOrderCommand(Guid OrderId, Guid SellerId) : IRequest<Result>;

public class FinalizeOrderCommandHandler : IRequestHandler<FinalizeOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMonobankService _monobank;
    private readonly IPublishEndpoint _publishEndpoint;

    public FinalizeOrderCommandHandler(
        IOrderRepository orderRepository,
        IMonobankService monobank,
        IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _monobank = monobank;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(FinalizeOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure("Замовлення не знайдено");

        if (order.SellerId != request.SellerId)
            return Result.Failure("Доступ заборонено");

        if (order.Status != OrderStatus.Hold)
            return Result.Failure($"Неможливо підтвердити: статус замовлення '{order.Status}'");

        if (string.IsNullOrEmpty(order.InvoiceId))
            return Result.Failure("InvoiceId відсутній");

        var success = await _monobank.FinalizeHoldAsync(order.InvoiceId, order.Amount, ct);

        if (!success)
        {
            try
            {
                var status = await _monobank.GetInvoiceStatusAsync(order.InvoiceId, ct);
                if (status.Status is not ("success" or "hold"))
                    return Result.Failure($"Неможливо завершити: статус інвойсу '{status.Status}'");
                if (status.Status == "hold")
                    return Result.Failure("Помилка списання коштів через Monobank. Спробуйте пізніше.");
            }
            catch
            {
                return Result.Failure("Помилка зв'язку з Monobank");
            }
        }

        order.MarkAsCompleted(DateTime.UtcNow);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OrderCompletedIntegrationEvent(
            order.Id, order.AdvertisementId, order.BuyerId, order.SellerId, order.Amount), ct);

        return Result.Success();
    }
}
