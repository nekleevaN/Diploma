using FluentAssertions;
using NSubstitute;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using Xunit;

namespace TrustMarket.ReviewService.UnitTests.Reviews;

public class ExpireOldReviewsCommandHandlerTests
{
    private readonly IReviewRepository _repo = Substitute.For<IReviewRepository>();

    private ExpireOldReviewsCommandHandler CreateHandler() => new(_repo);

    [Fact]
    public async Task Handle_NoneExpired_ReturnsZeroAndDoesNotSave()
    {
        _repo.GetPendingExpiredAsync(Arg.Any<DateTime>(), default).Returns(new List<Review>());

        var result = await CreateHandler().Handle(new ExpireOldReviewsCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SomeExpired_MarksThemExpiredAndSaves()
    {
        var r1 = Review.CreatePlaceholder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "А", ReviewType.BuyerToSeller);
        var r2 = Review.CreatePlaceholder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Б", ReviewType.SellerToBuyer);
        _repo.GetPendingExpiredAsync(Arg.Any<DateTime>(), default).Returns(new List<Review> { r1, r2 });

        var result = await CreateHandler().Handle(new ExpireOldReviewsCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        r1.Status.Should().Be(ReviewStatus.Expired);
        r2.Status.Should().Be(ReviewStatus.Expired);
        _repo.Received(1).Update(r1);
        _repo.Received(1).Update(r2);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
