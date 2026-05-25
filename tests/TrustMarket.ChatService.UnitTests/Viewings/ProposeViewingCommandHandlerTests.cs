using FluentAssertions;
using NSubstitute;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Application.Viewings.Commands;
using TrustMarket.ChatService.Domain.Entities;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.Viewings;

public class ProposeViewingCommandHandlerTests
{
    private readonly IViewingRequestRepository _viewingRepo = Substitute.For<IViewingRequestRepository>();
    private readonly IChatRepository _chatRepo = Substitute.For<IChatRepository>();

    private ProposeViewingCommandHandler CreateHandler() => new(_viewingRepo, _chatRepo);

    private static ProposeViewingCommand ValidCmd() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Телефон", "Київ", DateTime.UtcNow.AddDays(1));

    [Fact]
    public async Task Handle_HappyPath_CreatesViewingAndSavesSystemMessage()
    {
        var cmd = ValidCmd();

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _viewingRepo.Received(1).AddAsync(
            Arg.Is<ViewingRequest>(v =>
                v.ProposerId == cmd.ProposerId &&
                v.ResponderId == cmd.ResponderId &&
                v.Status == ViewingStatus.Pending),
            Arg.Any<CancellationToken>());
        await _viewingRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _chatRepo.Received(1).SaveMessageAsync(
            Arg.Is<Message>(m => m.ChatId == cmd.ChatId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BuildProposalMessageContent_ContainsViewingIdAndResponderId()
    {
        var viewingId = Guid.NewGuid();
        var responderId = Guid.NewGuid();
        var dt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        var content = ProposeViewingCommandHandler.BuildProposalMessageContent(viewingId, dt, responderId);

        content.Should().Contain(viewingId.ToString());
        content.Should().Contain(responderId.ToString());
        content.Should().Contain("viewing_proposal");
    }
}
