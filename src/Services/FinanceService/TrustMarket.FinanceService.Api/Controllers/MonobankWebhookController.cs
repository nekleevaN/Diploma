using MediatR;
using Microsoft.AspNetCore.Mvc;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Webhooks;

namespace TrustMarket.FinanceService.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class MonobankWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MonobankWebhookController> _logger;

    public MonobankWebhookController(IMediator mediator, ILogger<MonobankWebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("monobank")]
    public async Task<IActionResult> HandleMonobankWebhook(
        [FromBody] MonobankInvoiceStatus webhook, CancellationToken ct)
    {
        _logger.LogInformation(
            "Webhook від Monobank: InvoiceId={InvoiceId}, Status={Status}, Ref={Reference}",
            webhook.InvoiceId, webhook.Status, webhook.Reference);


        await _mediator.Send(new ProcessMonobankWebhookCommand(webhook), ct);

        return Ok();
    }
}
