using FluentAssertions;
using NSubstitute;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Application.Viewings.Commands;
using TrustMarket.ChatService.Domain.Entities;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.Viewings;

public class ViewingFollowUpResponseCommandHandlerTests
{
    private readonly IViewingRequestRepository _viewingRepo = Substitute.For<IViewingRequestRepository>();
    private readonly IChatRepository _chatRepo = Substitute.For<IChatRepository>();

    private ViewingFollowUpResponseCommandHandler CreateHandler() =>
        new(_viewingRepo, _chatRepo);

    private static ViewingRequest MakeViewing() =>
        ViewingRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(),
            "Товар", null, DateTime.UtcNow.AddDays(-1));

    [Fact]
    public async Task Handle_ViewingNotFound_ReturnsFailure()
    {
        _viewingRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((ViewingRequest?)null);

        var result = await CreateHandler().Handle(
            new ViewingFollowUpResponseCommand(Guid.NewGuid(), Guid.NewGuid(), "buy"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Theory]
    [InlineData("buy")]
    [InlineData("buy_delivery")]
    [InlineData("cancelled")]
    public async Task Handle_KnownAction_SetsFollowUpActionAndPersists(string action)
    {
        var vr = MakeViewing();
        var userId = Guid.NewGuid();
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);

        var result = await CreateHandler().Handle(
            new ViewingFollowUpResponseCommand(vr.Id, userId, action), default);

        result.IsSuccess.Should().BeTrue();
        vr.FollowUpAction.Should().Be(action);
        _viewingRepo.Received(1).Update(vr);
        await _viewingRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
