using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Payments.Commands;

namespace TrustMarket.CatalogService.Api.Controllers;

[ApiController]
[Route("api")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAdvertisementRepository _repository;

    public PaymentController(IMediator mediator, IAdvertisementRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    [HttpPost("ads/{id:guid}/pay")]
    [Authorize]
    public async Task<IActionResult> Pay(Guid id, CancellationToken ct)
    {
        var buyerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var result = await _mediator.Send(new CreatePaymentCommand(id, buyerId, baseUrl), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("payment/webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (!root.TryGetProperty("status", out var statusProp)) return Ok();
        var status = statusProp.GetString();

        if (status != "success") return Ok();

        if (!root.TryGetProperty("reference", out var refProp)) return Ok();
        var reference = refProp.GetString();

        if (reference?.StartsWith("AD-") == true &&
            Guid.TryParse(reference[3..], out var adId))
        {
            var ad = await _repository.GetByIdAsync(adId, ct);
            if (ad is not null)
            {
                ad.MarkAsSold();
                _repository.Update(ad);
                await _repository.SaveChangesAsync(ct);
            }
        }

        return Ok();
    }
}
