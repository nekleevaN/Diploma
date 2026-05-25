using MassTransit;
using MediatR;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Webhooks;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.FinanceService.Application.Orders.Commands;

public record SyncOrderStatusCommand(Guid OrderId, Guid RequesterId) : IRequest<Result<string>>;

public class SyncOrderStatusCommandHandler : IRequestHandler<SyncOrderStatusCommand, Result<string>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IMonobankService _monobank;
    private readonly IMediator _mediator;

    public SyncOrderStatusCommandHandler(
        IOrderRepository orderRepo,
        IMonobankService monobank,
        IMediator mediator)
    {
        _orderRepo = orderRepo;
        _monobank = monobank;
        _mediator = mediator;
    }

    public async Task<Result<string>> Handle(SyncOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure<string>("Замовлення не знайдено");

        if (order.BuyerId != request.RequesterId && order.SellerId != request.RequesterId)
            return Result.Failure<string>("Доступ заборонено");

        if (string.IsNullOrEmpty(order.InvoiceId))
            return Result.Failure<string>("Інвойс ще не створено");

        var status = await _monobank.GetInvoiceStatusAsync(order.InvoiceId, ct);

        await _mediator.Send(new ProcessMonobankWebhookCommand(new MonobankInvoiceStatus
        {
            InvoiceId = order.InvoiceId,
            Status = status.Status,
            Amount = status.Amount,
            Ccy = status.Ccy,
            Reference = order.Id.ToString(),
            ModifiedDate = status.ModifiedDate
        }), ct);

        return Result.Success(status.Status);
    }
}
