using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.FinanceService.Application.Orders.Commands;

public record CreateOrderCommand(
    Guid AdvertisementId,
    Guid BuyerId,
    Guid SellerId,
    string AdTitle,
    decimal Amount,
    bool HasDelivery = true) : IRequest<Result<CreateOrderResponse>>;

public record CreateOrderResponse(Guid OrderId, string PageUrl);

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMonobankService _monobank;
    private readonly IConfiguration _configuration;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMonobankService monobank,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _monobank = monobank;
        _configuration = configuration;
    }

    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (request.BuyerId == request.SellerId)
            return Result.Failure<CreateOrderResponse>("Не можна купити власне оголошення");

        var existing = await _orderRepository.GetByAdvertisementAndBuyerAsync(
            request.AdvertisementId, request.BuyerId, ct);

        if (existing is not null && existing.Status is OrderStatus.Pending or OrderStatus.Hold)
            return Result.Failure<CreateOrderResponse>("Активне замовлення вже існує");

        var order = Order.Create(
            request.AdvertisementId, request.BuyerId, request.SellerId,
            request.AdTitle, request.Amount, request.HasDelivery);

        await _orderRepository.AddAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        var webhookBase = _configuration["Monobank:WebhookBaseUrl"] ?? "";
        var redirectBase = _configuration["Monobank:RedirectBaseUrl"] ?? "http://localhost:3000";

        var webhookUrl = string.IsNullOrWhiteSpace(webhookBase)
            ? ""
            : $"{webhookBase.TrimEnd('/')}/api/webhooks/monobank";

        var redirectUrl = $"{redirectBase.TrimEnd('/')}/payment/success?orderId={order.Id}&hasDelivery={request.HasDelivery.ToString().ToLower()}";

        var invoice = await _monobank.CreateHoldInvoiceAsync(
            amount: request.Amount,
            reference: order.Id.ToString(),
            description: request.AdTitle,
            redirectUrl: redirectUrl,
            webhookUrl: webhookUrl,
            ct: ct);

        order.SetInvoiceId(invoice.InvoiceId);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(ct);

        return Result.Success(new CreateOrderResponse(order.Id, invoice.PageUrl));
    }
}
