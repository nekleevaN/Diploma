using MediatR;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ChatService.Application.Chats.Queries;

public record GetChatByIdQuery(Guid ChatId, Guid RequestingUserId) : IRequest<Result<ChatDetailDto>>;

public record MessageDto(
    Guid MessageId,
    Guid SenderId,
    string Content,
    DateTime SentAt,
    bool IsBlocked,
    bool IsFlagged,
    string? FraudReason);

public record ChatDetailDto(
    Guid ChatId,
    Guid BuyerId,
    Guid SellerId,
    Guid AdvertisementId,
    string AdTitle,
    List<MessageDto> Messages);

public class GetChatByIdQueryHandler : IRequestHandler<GetChatByIdQuery, Result<ChatDetailDto>>
{
    private readonly IChatRepository _chatRepository;

    public GetChatByIdQueryHandler(IChatRepository chatRepository)
        => _chatRepository = chatRepository;

    public async Task<Result<ChatDetailDto>> Handle(GetChatByIdQuery request, CancellationToken ct)
    {
        var chat = await _chatRepository.GetByIdAsync(request.ChatId, ct);
        if (chat is null)
            return Result.Failure<ChatDetailDto>("Чат не знайдено");

        if (chat.BuyerId != request.RequestingUserId && chat.SellerId != request.RequestingUserId)
            return Result.Failure<ChatDetailDto>("Доступ заборонено");

        var messages = chat.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto(
                m.Id,
                m.SenderId,
                m.Content,
                m.SentAt,
                m.IsBlocked,
                m.IsFlagged,
                m.IsBlocked || m.IsFlagged ? m.FraudReason : null))
            .ToList();

        return Result.Success(new ChatDetailDto(
            chat.Id, chat.BuyerId, chat.SellerId, chat.AdvertisementId, chat.AdTitle, messages));
    }
}
