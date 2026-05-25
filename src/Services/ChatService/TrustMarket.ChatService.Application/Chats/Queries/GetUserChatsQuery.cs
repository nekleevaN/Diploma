using MediatR;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ChatService.Application.Chats.Queries;

public record GetUserChatsQuery(Guid UserId) : IRequest<Result<List<ChatSummaryDto>>>;

public record ChatSummaryDto(
    Guid ChatId,
    Guid BuyerId,
    Guid SellerId,
    Guid AdvertisementId,
    string AdTitle,
    int MessageCount,
    DateTime CreatedAt);

public class GetUserChatsQueryHandler : IRequestHandler<GetUserChatsQuery, Result<List<ChatSummaryDto>>>
{
    private readonly IChatRepository _chatRepository;

    public GetUserChatsQueryHandler(IChatRepository chatRepository)
        => _chatRepository = chatRepository;

    public async Task<Result<List<ChatSummaryDto>>> Handle(GetUserChatsQuery request, CancellationToken ct)
    {
        var chats = await _chatRepository.GetByUserIdAsync(request.UserId, ct);

        var result = chats.Select(c => new ChatSummaryDto(
            c.Id,
            c.BuyerId,
            c.SellerId,
            c.AdvertisementId,
            c.AdTitle,
            c.Messages.Count,
            c.CreatedAt)).ToList();

        return Result.Success(result);
    }
}
