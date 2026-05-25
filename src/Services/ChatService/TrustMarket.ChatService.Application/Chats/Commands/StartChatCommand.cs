using MediatR;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ChatService.Application.Chats.Commands;

public record StartChatCommand(
    Guid BuyerId,
    Guid SellerId,
    Guid AdvertisementId,
    string AdTitle) : IRequest<Result<StartChatResponse>>;

public record StartChatResponse(Guid ChatId, bool IsNew);

public class StartChatCommandHandler : IRequestHandler<StartChatCommand, Result<StartChatResponse>>
{
    private readonly IChatRepository _chatRepository;

    public StartChatCommandHandler(IChatRepository chatRepository)
        => _chatRepository = chatRepository;

    public async Task<Result<StartChatResponse>> Handle(StartChatCommand request, CancellationToken ct)
    {
        if (request.BuyerId == request.SellerId)
            return Result.Failure<StartChatResponse>("Не можна створити чат із самим собою");

        var existing = await _chatRepository.GetByParticipantsAndAdAsync(
            request.BuyerId, request.SellerId, request.AdvertisementId, ct);

        if (existing is not null)
            return Result.Success(new StartChatResponse(existing.Id, IsNew: false));

        var chat = Chat.Create(request.BuyerId, request.SellerId, request.AdvertisementId, request.AdTitle);
        await _chatRepository.AddAsync(chat, ct);
        await _chatRepository.SaveChangesAsync(ct);

        return Result.Success(new StartChatResponse(chat.Id, IsNew: true));
    }
}
