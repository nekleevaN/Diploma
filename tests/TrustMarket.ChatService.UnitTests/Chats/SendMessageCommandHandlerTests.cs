using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Application.Chats.Commands;
using TrustMarket.ChatService.Application.FraudDetection;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.Chats;

public class SendMessageCommandHandlerTests
{
    private readonly IChatRepository _chatRepo = Substitute.For<IChatRepository>();
    private readonly IFraudAnalyzer _fraudAnalyzer = Substitute.For<IFraudAnalyzer>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    private SendMessageCommandHandler CreateHandler() =>
        new(_chatRepo, _fraudAnalyzer, _publisher);

    private static Chat MakeChat(Guid buyerId, Guid sellerId) =>
        Chat.Create(buyerId, sellerId, Guid.NewGuid(), "Товар");

    private static FraudAnalysisResult CleanResult() =>
        new(0, null, new List<string>());

    private static FraudAnalysisResult SuspiciousResult() =>
        new(80, "Виявлено: телеграм", new List<string> { "platform:telegram" });

    [Fact]
    public async Task Handle_ChatNotFound_ReturnsFailure()
    {
        _chatRepo.GetByParticipantsValidation(Arg.Any<Guid>(), default).Returns((Chat?)null);

        var result = await CreateHandler().Handle(
            new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), "text"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_SenderNotParticipant_ReturnsFailure()
    {
        var chat = MakeChat(Guid.NewGuid(), Guid.NewGuid());
        _chatRepo.GetByParticipantsValidation(chat.Id, default).Returns(chat);

        var result = await CreateHandler().Handle(
            new SendMessageCommand(chat.Id, Guid.NewGuid(), "text"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("учасником");
    }

    [Fact]
    public async Task Handle_CleanMessage_SavesMessageWithoutPublishing()
    {
        var buyerId = Guid.NewGuid();
        var chat = MakeChat(buyerId, Guid.NewGuid());
        _chatRepo.GetByParticipantsValidation(chat.Id, default).Returns(chat);
        _fraudAnalyzer.Analyze(Arg.Any<string>()).Returns(CleanResult());

        var result = await CreateHandler().Handle(
            new SendMessageCommand(chat.Id, buyerId, "Привіт!"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsBlocked.Should().BeFalse();
        await _chatRepo.Received(1).SaveMessageAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SuspiciousMessage_PublishesFraudEvent()
    {
        var buyerId = Guid.NewGuid();
        var chat = MakeChat(buyerId, Guid.NewGuid());
        _chatRepo.GetByParticipantsValidation(chat.Id, default).Returns(chat);
        _fraudAnalyzer.Analyze(Arg.Any<string>()).Returns(SuspiciousResult());

        var result = await CreateHandler().Handle(
            new SendMessageCommand(chat.Id, buyerId, "Давай в телеграм"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsBlocked.Should().BeTrue();
        await _publisher.Received(1).Publish(
            Arg.Any<SuspiciousMessageDetectedIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidMessage_ReturnsFraudScoreInResponse()
    {
        var buyerId = Guid.NewGuid();
        var chat = MakeChat(buyerId, Guid.NewGuid());
        _chatRepo.GetByParticipantsValidation(chat.Id, default).Returns(chat);
        _fraudAnalyzer.Analyze(Arg.Any<string>()).Returns(new FraudAnalysisResult(50, "підозріло", new()));

        var result = await CreateHandler().Handle(
            new SendMessageCommand(chat.Id, buyerId, "текст"), default);

        result.Value!.FraudScore.Should().Be(50);
        result.Value.IsFlagged.Should().BeTrue();
        result.Value.IsBlocked.Should().BeFalse();
    }
}
