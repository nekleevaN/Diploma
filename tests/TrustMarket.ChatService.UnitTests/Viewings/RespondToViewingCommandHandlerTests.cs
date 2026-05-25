using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Application.Viewings.Commands;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.Viewings;

public class RespondToViewingCommandHandlerTests
{
    private readonly IViewingRequestRepository _viewingRepo = Substitute.For<IViewingRequestRepository>();
    private readonly IChatRepository _chatRepo = Substitute.For<IChatRepository>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    private RespondToViewingCommandHandler CreateHandler() =>
        new(_viewingRepo, _chatRepo, _publisher);

    private static ViewingRequest MakeViewing(Guid proposerId, Guid responderId) =>
        ViewingRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            proposerId, responderId,
            "Товар", "Київ", DateTime.UtcNow.AddDays(1));

    [Fact]
    public async Task Handle_ViewingNotFound_ReturnsFailure()
    {
        _viewingRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((ViewingRequest?)null);

        var result = await CreateHandler().Handle(
            new RespondToViewingCommand(Guid.NewGuid(), Guid.NewGuid(), "accept", null, null, null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongResponder_ReturnsFailure()
    {
        var vr = MakeViewing(Guid.NewGuid(), Guid.NewGuid());
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);

        var result = await CreateHandler().Handle(
            new RespondToViewingCommand(vr.Id, Guid.NewGuid(), "accept", null, null, null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_Accept_SetsAcceptedAndPublishesEvent()
    {
        var proposerId = Guid.NewGuid();
        var responderId = Guid.NewGuid();
        var vr = MakeViewing(proposerId, responderId);
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);

        var result = await CreateHandler().Handle(
            new RespondToViewingCommand(vr.Id, responderId, "accept", null, null, "Продавець", "Покупець"), default);

        result.IsSuccess.Should().BeTrue();
        vr.Status.Should().Be(ViewingStatus.Accepted);
        await _publisher.Received(1).Publish(
            Arg.Any<ViewingConfirmedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Decline_SetsDeclinedStatus()
    {
        var responderId = Guid.NewGuid();
        var vr = MakeViewing(Guid.NewGuid(), responderId);
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);

        var result = await CreateHandler().Handle(
            new RespondToViewingCommand(vr.Id, responderId, "decline", null, null, null, null), default);

        result.IsSuccess.Should().BeTrue();
        vr.Status.Should().Be(ViewingStatus.Declined);
    }

    [Fact]
    public async Task Handle_Reschedule_SwapsRolesAndSetsPending()
    {
        var proposerId = Guid.NewGuid();
        var responderId = Guid.NewGuid();
        var vr = MakeViewing(proposerId, responderId);
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);
        var newDate = DateTime.UtcNow.AddDays(3);

        var result = await CreateHandler().Handle(
            new RespondToViewingCommand(vr.Id, responderId, "reschedule", newDate, null, null, null), default);

        result.IsSuccess.Should().BeTrue();
        vr.Status.Should().Be(ViewingStatus.Pending);
        vr.ProposedDateTime.Should().Be(newDate);
    }

    [Fact]
    public async Task Handle_RescheduleWithoutDate_ReturnsFailure()
    {
        var responderId = Guid.NewGuid();
        var vr = MakeViewing(Guid.NewGuid(), responderId);
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);

        var result = await CreateHandler().Handle(
            new RespondToViewingCommand(vr.Id, responderId, "reschedule", null, null, null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("нову дату");
    }

    [Fact]
    public async Task Handle_UnknownAction_ReturnsFailure()
    {
        var responderId = Guid.NewGuid();
        var vr = MakeViewing(Guid.NewGuid(), responderId);
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);

        var result = await CreateHandler().Handle(
            new RespondToViewingCommand(vr.Id, responderId, "unknown", null, null, null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Невідома");
    }

    [Fact]
    public async Task Handle_ValidResponse_SavesChangesAndSystemMessage()
    {
        var responderId = Guid.NewGuid();
        var vr = MakeViewing(Guid.NewGuid(), responderId);
        _viewingRepo.GetByIdAsync(vr.Id, default).Returns(vr);

        await CreateHandler().Handle(
            new RespondToViewingCommand(vr.Id, responderId, "decline", null, null, null, null), default);

        _viewingRepo.Received(1).Update(vr);
        await _viewingRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _chatRepo.Received(1).SaveMessageAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }
}
