using MassTransit;
using MediatR;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Application.FraudDetection;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.ChatService.Application.Chats.Commands;

public record SendMessageCommand(
    Guid ChatId,
    Guid SenderId,
    string Content) : IRequest<Result<SendMessageResponse>>;

public record SendMessageResponse(
    Guid MessageId,
    int FraudScore,
    string? FraudReason,
    bool IsBlocked,
    bool IsFlagged);

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<SendMessageResponse>>
{
    private readonly IChatRepository _chatRepository;
    private readonly IFraudAnalyzer _fraudAnalyzer;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendMessageCommandHandler(
        IChatRepository chatRepository,
        IFraudAnalyzer fraudAnalyzer,
        IPublishEndpoint publishEndpoint)
    {
        _chatRepository = chatRepository;
        _fraudAnalyzer = fraudAnalyzer;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<SendMessageResponse>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var chat = await _chatRepository.GetByParticipantsValidation(request.ChatId, ct);
        if (chat is null)
            return Result.Failure<SendMessageResponse>("Чат не знайдено");

        if (!chat.IsSender(request.SenderId))
            return Result.Failure<SendMessageResponse>("Ви не є учасником цього чату");

        var fraudResult = _fraudAnalyzer.Analyze(request.Content);

        var message = Message.Create(
            request.ChatId,
            request.SenderId,
            request.Content,
            fraudResult.Score,
            fraudResult.Reason);

        await _chatRepository.SaveMessageAsync(message, ct);

        if (!fraudResult.IsClean)
        {
            await _publishEndpoint.Publish(new SuspiciousMessageDetectedIntegrationEvent(
                message.Id,
                request.SenderId,
                request.ChatId,
                fraudResult.Reason ?? "Підозріла активність",
                fraudResult.Score,
                DateTime.UtcNow), ct);
        }

        return Result.Success(new SendMessageResponse(
            message.Id,
            message.FraudScore,
            message.FraudReason,
            message.IsBlocked,
            message.IsFlagged));
    }
}
