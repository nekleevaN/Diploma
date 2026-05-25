using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustMarket.ChatService.Application.Viewings.Commands;

namespace TrustMarket.ChatService.Api.Controllers;

[ApiController]
[Route("api/viewings")]
[Authorize]
public class ViewingController : ControllerBase
{
    private readonly IMediator _mediator;
    public ViewingController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Propose([FromBody] ProposeViewingRequest request, CancellationToken ct)
    {
        var displayName = User.FindFirstValue("display_name") ?? User.FindFirstValue("username");
        var result = await _mediator.Send(new ProposeViewingCommand(
            request.ChatId, request.AdvertisementId,
            CurrentUserId, request.ResponderId,
            request.AdTitle, request.LocationAddress,
            request.ProposedDateTime,
            ProposerDisplayName: displayName,
            request.ProposerTrustedTelegramId,
            request.ProposerTrustedEmail), ct);

        return result.IsSuccess ? Ok(new { viewingId = result.Value }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{viewingId:guid}/respond")]
    public async Task<IActionResult> Respond(Guid viewingId, [FromBody] RespondViewingRequest request, CancellationToken ct)
    {
        var displayName = User.FindFirstValue("display_name") ?? User.FindFirstValue("username") ?? "Учасник";
        var result = await _mediator.Send(new RespondToViewingCommand(
            viewingId, CurrentUserId, request.Action,
            request.NewDateTime,
            request.ResponderTrustedTelegramId,
            ResponderName: displayName,
            ProposerName: request.ProposerName,
            ResponderTrustedEmail: request.ResponderTrustedEmail), ct);

        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{viewingId:guid}/followup")]
    public async Task<IActionResult> FollowUp(Guid viewingId, [FromBody] FollowUpRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ViewingFollowUpResponseCommand(viewingId, CurrentUserId, request.Action), ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }
}

public record ProposeViewingRequest(
    Guid ChatId, Guid AdvertisementId, Guid ResponderId,
    string AdTitle, string? LocationAddress, DateTime ProposedDateTime,
    long? ProposerTrustedTelegramId = null,
    string? ProposerTrustedEmail = null);

public record RespondViewingRequest(
    string Action, DateTime? NewDateTime,
    long? ResponderTrustedTelegramId,
    string? ProposerName,
    string? ResponderTrustedEmail = null);

public record FollowUpRequest(string Action);
