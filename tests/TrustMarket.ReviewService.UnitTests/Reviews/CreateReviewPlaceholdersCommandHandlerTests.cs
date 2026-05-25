using FluentAssertions;
using NSubstitute;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using Xunit;

namespace TrustMarket.ReviewService.UnitTests.Reviews;

public class CreateReviewPlaceholdersCommandHandlerTests
{
    private readonly IReviewRepository _repo = Substitute.For<IReviewRepository>();

    private CreateReviewPlaceholdersCommandHandler CreateHandler() => new(_repo);

    private static CreateReviewPlaceholdersCommand ValidCmd() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Покупець", "Продавець");

    [Fact]
    public async Task Handle_AlreadyExists_SkipsCreationAndSucceeds()
    {
        var cmd = ValidCmd();
        var existing = new List<Review>
        {
            Review.CreatePlaceholder(cmd.OrderId, cmd.BuyerId, cmd.SellerId, "B", ReviewType.BuyerToSeller)
        };
        _repo.GetByOrderIdAsync(cmd.OrderId, default).Returns(existing);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        await _repo.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoneExist_CreatesBothPlaceholders()
    {
        var cmd = ValidCmd();
        _repo.GetByOrderIdAsync(cmd.OrderId, default).Returns(new List<Review>());

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(2).AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoneExist_CreatesBuyerToSellerAndSellerToBuyerTypes()
    {
        var cmd = ValidCmd();
        _repo.GetByOrderIdAsync(cmd.OrderId, default).Returns(new List<Review>());

        await CreateHandler().Handle(cmd, default);

        await _repo.Received(1).AddAsync(
            Arg.Is<Review>(r => r.Type == ReviewType.BuyerToSeller && r.ReviewerId == cmd.BuyerId),
            Arg.Any<CancellationToken>());
        await _repo.Received(1).AddAsync(
            Arg.Is<Review>(r => r.Type == ReviewType.SellerToBuyer && r.ReviewerId == cmd.SellerId),
            Arg.Any<CancellationToken>());
    }
}
