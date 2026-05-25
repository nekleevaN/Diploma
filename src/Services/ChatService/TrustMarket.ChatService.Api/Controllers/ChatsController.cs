using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustMarket.ChatService.Application.Chats.Commands;
using TrustMarket.ChatService.Application.Chats.Queries;

namespace TrustMarket.ChatService.Api.Controllers;

[ApiController]
[Route("api/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatsController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> StartChat([FromBody] StartChatRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new StartChatCommand(CurrentUserId, request.SellerId, request.AdvertisementId, request.AdTitle ?? "Оголошення"), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyChats(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserChatsQuery(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{chatId:guid}")]
    public async Task<IActionResult> GetChat(Guid chatId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChatByIdQuery(chatId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{chatId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SendMessageCommand(chatId, CurrentUserId, request.Content), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var msg = result.Value!;

        return Ok(result.Value);
    }
}

public record StartChatRequest(Guid SellerId, Guid AdvertisementId, string? AdTitle);
public record SendMessageRequest(string Content);
