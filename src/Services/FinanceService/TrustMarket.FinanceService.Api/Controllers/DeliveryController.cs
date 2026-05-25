using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustMarket.FinanceService.Application.Delivery.Commands;
using TrustMarket.FinanceService.Application.Delivery.Queries;

namespace TrustMarket.FinanceService.Api.Controllers;

[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly IMediator _mediator;
    public DeliveryController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("cities")]
    public async Task<IActionResult> SearchCities([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<object>());
        var cities = await _mediator.Send(new SearchCitiesQuery(q), ct);
        return Ok(cities);
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses(
        [FromQuery] string cityRef,
        [FromQuery] int page = 1,
        [FromQuery] string? q = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cityRef))
            return BadRequest(new { error = "cityRef обов'язковий" });
        var warehouses = await _mediator.Send(new GetWarehousesQuery(cityRef, page, q), ct);
        return Ok(warehouses);
    }

    [HttpPost("{orderId:guid}/recipient")]
    [Authorize]
    public async Task<IActionResult> SetRecipientAddress(
        Guid orderId, [FromBody] SetRecipientRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetRecipientAddressCommand(
            orderId, CurrentUserId,
            request.CityRef, request.CityName,
            request.WarehouseRef, request.WarehouseAddress,
            request.RecipientName, request.RecipientPhone), ct);
        return result.IsSuccess ? Ok(new { deliveryId = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{orderId:guid}/sender")]
    [Authorize]
    public async Task<IActionResult> SetSenderAddress(
        Guid orderId, [FromBody] SetSenderRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetSenderAddressCommand(
            orderId, CurrentUserId,
            request.CityRef, request.CityName,
            request.WarehouseRef, request.WarehouseAddress,
            request.SenderName, request.SenderPhone), ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{orderId:guid}/generate-ttn")]
    [Authorize]
    public async Task<IActionResult> GenerateTTN(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateTTNCommand(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(new { ttn = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{orderId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetDelivery(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDeliveryByOrderQuery(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{orderId:guid}/track")]
    [Authorize]
    public async Task<IActionResult> Track(Guid orderId, CancellationToken ct)
    {
        var result = await _mediator.Send(new TrackDeliveryQuery(orderId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}

public record SetRecipientRequest(
    string CityRef, string CityName,
    string WarehouseRef, string WarehouseAddress,
    string RecipientName, string RecipientPhone);

public record SetSenderRequest(
    string CityRef, string CityName,
    string WarehouseRef, string WarehouseAddress,
    string SenderName, string SenderPhone);
