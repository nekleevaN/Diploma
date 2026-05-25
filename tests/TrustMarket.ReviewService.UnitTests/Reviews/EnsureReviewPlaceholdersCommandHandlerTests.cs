using FluentAssertions;
using NSubstitute;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using Xunit;

namespace TrustMarket.ReviewService.UnitTests.Reviews;

public class EnsureReviewPlaceholdersCommandHandlerTests
{
    private readonly IReviewRepository _repo = Substitute.For<IReviewRepository>();

    private EnsureReviewPlaceholdersCommandHandler CreateHandler() => new(_repo);

    private static EnsureReviewPlaceholdersCommand MakeCmd(Guid buyerId, Guid sellerId, Guid? currentUser = null) =>
        new(Guid.NewGuid(), buyerId, sellerId, currentUser ?? buyerId, "Покупець", "Продавець");

    [Fact]
    public async Task Handle_CurrentUserNotParticipant_ReturnsFailure()
    {
        var cmd = MakeCmd(Guid.NewGuid(), Guid.NewGuid(), currentUser: Guid.NewGuid());

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("учасником");
    }

    [Fact]
    public async Task Handle_NoExistingPlaceholders_CreatesBothAndReturnsMyReviewId()
    {
        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var cmd = MakeCmd(buyerId, sellerId, currentUser: buyerId);
        _repo.GetByOrderIdAsync(cmd.OrderId, default).Returns(new List<Review>());

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repo.Received(2).AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingPlaceholders_ReturnsBuyerReviewIdWithoutCreating()
    {
        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var cmd = MakeCmd(buyerId, sellerId, currentUser: buyerId);
        var existing = new List<Review>
        {
            Review.CreatePlaceholder(cmd.OrderId, buyerId, sellerId, "Покупець", ReviewType.BuyerToSeller),
            Review.CreatePlaceholder(cmd.OrderId, sellerId, buyerId, "Продавець", ReviewType.SellerToBuyer)
        };
        _repo.GetByOrderIdAsync(cmd.OrderId, default).Returns(existing);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existing[0].Id);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MyReviewAlreadySubmitted_ReturnsFailure()
    {
        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var cmd = MakeCmd(buyerId, sellerId, currentUser: buyerId);
        var buyerReview = Review.CreatePlaceholder(cmd.OrderId, buyerId, sellerId, "Покупець", ReviewType.BuyerToSeller);
        buyerReview.Submit(5, null, false, null, null, null, "Покупець");
        _repo.GetByOrderIdAsync(cmd.OrderId, default).Returns(new List<Review> { buyerReview });

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("вже залишено");
    }
}
