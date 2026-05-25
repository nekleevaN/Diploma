using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TrustMarket.ChatService.Application.Chats.Commands;

namespace TrustMarket.ChatService.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator) => _mediator = mediator;

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task SendMessage(Guid chatId, string content)
    {
        var senderId = Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mediator.Send(new SendMessageCommand(chatId, senderId, content));

        if (result.IsFailure)
        {
            await Clients.Caller.SendAsync("Error", result.Error);
            return;
        }

        var msg = result.Value!;

        if (msg.IsBlocked)
        {
            await Clients.Caller.SendAsync("MessageBlocked", new
            {
                reason = "Повідомлення заблоковано антифрод-системою",
                fraudReason = msg.FraudReason
            });
            return;
        }

        var payload = new
        {
            messageId = msg.MessageId,
            chatId,
            senderId,
            content,
            isFlagged = msg.IsFlagged,
            fraudWarning = msg.IsFlagged ? msg.FraudReason : null
        };

        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", payload);
    }
}
