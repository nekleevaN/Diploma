using FluentAssertions;
using NSubstitute;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Application.Chats.Commands;
using TrustMarket.ChatService.Domain.Entities;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.Chats;

public class StartChatCommandHandlerTests
{
    private readonly IChatRepository _chatRepo = Substitute.For<IChatRepository>();

    private StartChatCommandHandler CreateHandler() => new(_chatRepo);

    private static StartChatCommand ValidCmd(Guid? buyerId = null, Guid? sellerId = null) =>
        new(buyerId ?? Guid.NewGuid(), sellerId ?? Guid.NewGuid(),
            Guid.NewGuid(), "Телефон Samsung");

    [Fact]
    public async Task Handle_BuyerEqualsSeller_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var cmd = ValidCmd(buyerId: userId, sellerId: userId);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("самим собою");
    }

    [Fact]
    public async Task Handle_ExistingChat_ReturnsExistingIdWithIsNewFalse()
    {
        var cmd = ValidCmd();
        var existing = Chat.Create(cmd.BuyerId, cmd.SellerId, cmd.AdvertisementId, cmd.AdTitle);
        _chatRepo.GetByParticipantsAndAdAsync(cmd.BuyerId, cmd.SellerId, cmd.AdvertisementId, default)
            .Returns(existing);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ChatId.Should().Be(existing.Id);
        result.Value.IsNew.Should().BeFalse();
        await _chatRepo.DidNotReceive().AddAsync(Arg.Any<Chat>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoExistingChat_CreatesAndSaves()
    {
        var cmd = ValidCmd();
        _chatRepo.GetByParticipantsAndAdAsync(cmd.BuyerId, cmd.SellerId, cmd.AdvertisementId, default)
            .Returns((Chat?)null);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsNew.Should().BeTrue();
        await _chatRepo.Received(1).AddAsync(Arg.Any<Chat>(), Arg.Any<CancellationToken>());
        await _chatRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
