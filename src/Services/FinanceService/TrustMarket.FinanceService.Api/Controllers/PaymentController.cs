using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustMarket.FinanceService.Application.Orders.Commands;
using TrustMarket.FinanceService.Application.Orders.Queries;
using TrustMarket.FinanceService.Application.Webhooks;

namespace TrustMarket.FinanceService.Api.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    public PaymentController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateOrderCommand(
                request.AdvertisementId, CurrentUserId,
                request.SellerId, request.AdTitle, request.Amount, request.HasDelivery), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{orderId:guid}/finalize")]
    public async Task<IActionResult> Finalize(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinalizeOrderCommand(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(new { message = "Кошти успішно списані" }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelOrderCommand(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(new { message = "Замовлення скасовано" }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{orderId:guid}/confirm-receipt")]
    public async Task<IActionResult> ConfirmReceipt(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmReceiptCommand(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(new { message = "Товар підтверджено, кошти надійдуть продавцю" }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{orderId:guid}/refund")]
    public async Task<IActionResult> Refund(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefundOrderCommand(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(new { message = "Кошти буде повернено на вашу картку" }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{orderId:guid}/sync-status")]
    public async Task<IActionResult> SyncStatus(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SyncOrderStatusCommand(orderId, CurrentUserId), ct);
        return result.IsSuccess
            ? Ok(new { status = result.Value, message = GetStatusMessage(result.Value) })
            : BadRequest(new { error = result.Error });
    }

    private static string GetStatusMessage(string? status) => status switch
    {
        "hold" => "✅ Оплачено! Кошти заморожено. Продавець отримав сповіщення.",
        "success" => "✅ Кошти успішно зараховано.",
        "processing" => "⏳ Обробляється...",
        "created" => "⏳ Очікує оплати. Перейдіть на сторінку Monobank.",
        "failure" => "❌ Помилка оплати.",
        "expired" => "❌ Час оплати вийшов.",
        _ => $"Статус: {status}"
    };

    [HttpGet("my/buyer")]
    public async Task<IActionResult> GetMyBuyerOrders(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyOrdersAsBuyerQuery(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("my/seller")]
    public async Task<IActionResult> GetMySellerOrders(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyOrdersAsSellerQuery(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}

public record CreateOrderRequest(Guid AdvertisementId, Guid SellerId, string AdTitle, decimal Amount, bool HasDelivery = true);


[ApiController]
[Route("api/orders")]
[Authorize(Policy = "EmailConfirmed")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CreateCheckoutOrder(
        [FromBody] CreateCheckoutOrderRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCheckoutOrderCommand(
            req.AdvertisementId, CurrentUserId, req.Amount,
            req.RecipientCityRef, req.RecipientCityName,
            req.RecipientWarehouseRef, req.RecipientWarehouseAddress,
            req.RecipientFirstName, req.RecipientLastName, req.RecipientPhone), ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("CONFLICT:"))
                return Conflict(new { error = result.Error[9..] });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            orderId = result.Value!.OrderId,
            monoPageUrl = result.Value.PageUrl
        });
    }
}

public record CreateCheckoutOrderRequest(
    Guid AdvertisementId,
    decimal Amount,
    string RecipientCityRef,
    string RecipientCityName,
    string RecipientWarehouseRef,
    string RecipientWarehouseAddress,
    string RecipientFirstName,
    string RecipientLastName,
    string RecipientPhone);
