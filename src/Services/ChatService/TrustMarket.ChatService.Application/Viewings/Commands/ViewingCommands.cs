using MassTransit;
using MediatR;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.ChatService.Application.Viewings.Commands;

public record ProposeViewingCommand(
    Guid ChatId,
    Guid AdvertisementId,
    Guid ProposerId,
    Guid ResponderId,
    string AdTitle,
    string? LocationAddress,
    DateTime ProposedDateTime,
    string? ProposerDisplayName = null,
    long? ProposerTrustedTelegramId = null,
    string? ProposerTrustedEmail = null) : IRequest<Result<Guid>>;

public record RespondToViewingCommand(
    Guid ViewingId,
    Guid ResponderId,
    string Action,
    DateTime? NewDateTime,
    long? ResponderTrustedTelegramId,
    string? ResponderName,
    string? ProposerName,
    string? ResponderTrustedEmail = null) : IRequest<Result>;

public record ViewingFollowUpResponseCommand(
    Guid ViewingId,
    Guid UserId,
    string Action) : IRequest<Result>;

public class ProposeViewingCommandHandler : IRequestHandler<ProposeViewingCommand, Result<Guid>>
{
    private readonly IViewingRequestRepository _repo;
    private readonly IChatRepository _chatRepo;

    public ProposeViewingCommandHandler(IViewingRequestRepository repo, IChatRepository chatRepo)
    {
        _repo = repo;
        _chatRepo = chatRepo;
    }

    public async Task<Result<Guid>> Handle(ProposeViewingCommand request, CancellationToken ct)
    {
        var viewing = ViewingRequest.Create(
            request.ChatId, request.AdvertisementId,
            request.ProposerId, request.ResponderId,
            request.AdTitle, request.LocationAddress,
            request.ProposedDateTime,
            request.ProposerTrustedTelegramId,
            request.ProposerTrustedEmail);

        await _repo.AddAsync(viewing, ct);
        await _repo.SaveChangesAsync(ct);

        var systemMessage = Message.Create(
            request.ChatId,
            request.ProposerId,
            BuildProposalMessageContent(viewing.Id, request.ProposedDateTime, request.ResponderId, request.ProposerDisplayName),
            0, null);

        await _chatRepo.SaveMessageAsync(systemMessage, ct);

        return Result.Success(viewing.Id);
    }

    public static string BuildProposalMessageContent(Guid viewingId, DateTime dt, Guid responderId, string? proposerName = null)
    {
        var nameJson = !string.IsNullOrEmpty(proposerName)
            ? $",\"proposerName\":\"{proposerName}\""
            : "";
        return $"{{\"type\":\"viewing_proposal\",\"viewingId\":\"{viewingId}\"," +
               $"\"dateTime\":\"{dt:O}\"," +
               $"\"responderId\":\"{responderId}\"{nameJson}}}";
    }
}

public class RespondToViewingCommandHandler : IRequestHandler<RespondToViewingCommand, Result>
{
    private readonly IViewingRequestRepository _repo;
    private readonly IChatRepository _chatRepo;
    private readonly IPublishEndpoint _publishEndpoint;

    public RespondToViewingCommandHandler(
        IViewingRequestRepository repo,
        IChatRepository chatRepo,
        IPublishEndpoint publishEndpoint)
    {
        _repo = repo;
        _chatRepo = chatRepo;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(RespondToViewingCommand request, CancellationToken ct)
    {
        var viewing = await _repo.GetByIdAsync(request.ViewingId, ct);
        if (viewing is null) return Result.Failure("Перегляд не знайдено");
        if (viewing.ResponderId != request.ResponderId) return Result.Failure("Доступ заборонено");

        string responseContent;

        switch (request.Action)
        {
            case "accept":
                viewing.Accept();
                viewing.SetResponderTrustedContact(request.ResponderTrustedTelegramId, request.ResponderTrustedEmail);
                responseContent = $"{{\"type\":\"viewing_accepted\",\"viewingId\":\"{viewing.Id}\"," +
                                  $"\"dateTime\":\"{viewing.ProposedDateTime:O}\"}}";

                await _publishEndpoint.Publish(new ViewingConfirmedIntegrationEvent(
                    viewing.Id,
                    viewing.ChatId,
                    BuyerId: viewing.ProposerId,
                    SellerId: viewing.ResponderId,
                    viewing.AdTitle,
                    BuyerName: request.ProposerName ?? "Покупець",
                    SellerName: request.ResponderName ?? "Продавець",
                    viewing.ProposedDateTime,
                    viewing.LocationAddress,
                    BuyerTrustedTelegramId: viewing.ProposerTrustedTelegramId,
                    SellerTrustedTelegramId: viewing.ResponderTrustedTelegramId,
                    BuyerTrustedEmail: viewing.ProposerTrustedEmail,
                    SellerTrustedEmail: viewing.ResponderTrustedEmail), ct);
                break;

            case "decline":
                viewing.Decline();
                responseContent = $"{{\"type\":\"viewing_declined\",\"viewingId\":\"{viewing.Id}\"}}";
                break;

            case "reschedule":
                if (!request.NewDateTime.HasValue) return Result.Failure("Вкажіть нову дату");
                viewing.Reschedule(request.NewDateTime.Value);
                responseContent = $"{{\"type\":\"viewing_proposal\",\"viewingId\":\"{viewing.Id}\"," +
                                  $"\"dateTime\":\"{request.NewDateTime.Value:O}\"," +
                                  $"\"responderId\":\"{viewing.ResponderId}\"}}";
                break;

            default:
                return Result.Failure("Невідома дія");
        }

        _repo.Update(viewing);
        await _repo.SaveChangesAsync(ct);

        var msg = Message.Create(viewing.ChatId, request.ResponderId, responseContent, 0, null);
        await _chatRepo.SaveMessageAsync(msg, ct);

        return Result.Success();
    }
}

public class ViewingFollowUpResponseCommandHandler : IRequestHandler<ViewingFollowUpResponseCommand, Result>
{
    private readonly IViewingRequestRepository _repo;

    public ViewingFollowUpResponseCommandHandler(IViewingRequestRepository repo)
        => _repo = repo;

    public async Task<Result> Handle(ViewingFollowUpResponseCommand request, CancellationToken ct)
    {
        var viewing = await _repo.GetByIdAsync(request.ViewingId, ct);
        if (viewing is null) return Result.Failure("Перегляд не знайдено");

        viewing.SetFollowUpAction(request.Action);
        _repo.Update(viewing);
        await _repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}
