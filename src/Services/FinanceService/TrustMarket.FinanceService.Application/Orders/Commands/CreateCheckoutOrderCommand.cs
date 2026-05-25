using MediatR;
using Microsoft.Extensions.Configuration;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.FinanceService.Application.Orders.Commands;

public record CreateCheckoutOrderCommand(
    Guid AdvertisementId,
    Guid BuyerId,
    decimal Amount,
    string RecipientCityRef,
    string RecipientCityName,
    string RecipientWarehouseRef,
    string RecipientWarehouseAddress,
    string RecipientFirstName,
    string RecipientLastName,
    string RecipientPhone) : IRequest<Result<CreateOrderResponse>>;

public class CreateCheckoutOrderCommandHandler
    : IRequestHandler<CreateCheckoutOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IOrderRepository _orders;
    private readonly IDeliveryRepository _deliveries;
    private readonly IMonobankService _monobank;
    private readonly ICatalogServiceClient _catalog;
    private readonly IConfiguration _config;

    public CreateCheckoutOrderCommandHandler(
        IOrderRepository orders,
        IDeliveryRepository deliveries,
        IMonobankService monobank,
        ICatalogServiceClient catalog,
        IConfiguration config)
    {
        _orders = orders;
        _deliveries = deliveries;
        _monobank = monobank;
        _catalog = catalog;
        _config = config;
    }

    public async Task<Result<CreateOrderResponse>> Handle(
        CreateCheckoutOrderCommand req, CancellationToken ct)
    {
        var reservation = await _catalog.ReserveAdvertisementAsync(req.AdvertisementId, ct);

        if (!reservation.IsSuccess)
        {
            var prefix = reservation.IsConflict ? "CONFLICT:" : "";
            return Result.Failure<CreateOrderResponse>(prefix + reservation.Error);
        }

        var ad = reservation.Data!;

        if (string.IsNullOrEmpty(ad.SellerSubMerchantId))
        {
            await _catalog.UnreserveAdvertisementAsync(req.AdvertisementId, ct);
            return Result.Failure<CreateOrderResponse>(
                "SELLER_NO_PAYOUT:Продавець ще не підключив отримання виплат через Monobank. " +
                "Покупка неможлива до налаштування.");
        }

        var existing = await _orders.GetByAdvertisementAndBuyerAsync(
            req.AdvertisementId, req.BuyerId, ct);

        if (existing is { Status: OrderStatus.Pending or OrderStatus.Hold })
        {
            await _catalog.UnreserveAdvertisementAsync(req.AdvertisementId, ct);
            return Result.Failure<CreateOrderResponse>("Активне замовлення вже існує");
        }

        var order = Order.Create(
            req.AdvertisementId, req.BuyerId, ad.SellerId, ad.Title, req.Amount);

        await _orders.AddAsync(order, ct);
        await _orders.SaveChangesAsync(ct);

        var delivery = Domain.Entities.Delivery.Create(order.Id, ad.SellerId, req.BuyerId);
        delivery.SetRecipientAddress(
            req.RecipientCityRef, req.RecipientCityName,
            req.RecipientWarehouseRef, req.RecipientWarehouseAddress,
            $"{req.RecipientFirstName} {req.RecipientLastName}",
            req.RecipientPhone);

        await _deliveries.AddAsync(delivery, ct);
        await _deliveries.SaveChangesAsync(ct);

        var platformFeePercent = decimal.Parse(_config["Platform:FeePercent"] ?? "5");
        var sellerAmount = Math.Round(req.Amount * (1 - platformFeePercent / 100), 2);
        var sellerAmountKop = (long)(sellerAmount * 100);

        IReadOnlyList<MonobankSplitRule>? splitRules = null;
        if (!string.IsNullOrEmpty(ad.SellerSubMerchantId) && sellerAmountKop > 0)
        {
            splitRules = new[]
            {
                new MonobankSplitRule(
                    SubMerchantId: ad.SellerSubMerchantId,
                    AmountKopecks: sellerAmountKop,
                    Description:   $"Виплата за: {ad.Title}")
            };
        }

        var webhookBase = _config["Monobank:WebhookBaseUrl"] ?? "";
        var redirectBase = _config["Monobank:RedirectBaseUrl"] ?? "http://localhost:3000";

        var webhookUrl = string.IsNullOrWhiteSpace(webhookBase)
            ? ""
            : $"{webhookBase.TrimEnd('/')}/api/webhooks/monobank";

        var redirectUrl = $"{redirectBase.TrimEnd('/')}/orders/{order.Id}/success";

        MonobankInvoiceResult invoice;
        try
        {
            invoice = await _monobank.CreateHoldInvoiceAsync(
                amount: req.Amount,
                reference: order.Id.ToString(),
                description: ad.Title,
                redirectUrl: redirectUrl,
                webhookUrl: webhookUrl,
                splitRules: splitRules,
                ct: ct);
        }
        catch (Exception)
        {
            await _catalog.UnreserveAdvertisementAsync(req.AdvertisementId, ct);
            return Result.Failure<CreateOrderResponse>(
                "Платіжна система тимчасово недоступна. Спробуйте ще раз.");
        }

        order.SetInvoiceId(invoice.InvoiceId);
        _orders.Update(order);
        await _orders.SaveChangesAsync(ct);

        return Result.Success(new CreateOrderResponse(order.Id, invoice.PageUrl));
    }
}
