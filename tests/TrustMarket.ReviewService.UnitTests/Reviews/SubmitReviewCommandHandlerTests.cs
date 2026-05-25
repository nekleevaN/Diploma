using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.ReviewService.UnitTests.Reviews;

public class SubmitReviewCommandHandlerTests
{
    private readonly IReviewRepository _repo = Substitute.For<IReviewRepository>();
    private readonly IPublishEndpoint _bus = Substitute.For<IPublishEndpoint>();

    private SubmitReviewCommandHandler CreateHandler() => new(_repo, _bus);

    private static Review MakePendingReview(Guid reviewerId) =>
        Review.CreatePlaceholder(Guid.NewGuid(), reviewerId, Guid.NewGuid(), "Іван", ReviewType.BuyerToSeller);

    private static SubmitReviewCommand ValidCmd(Guid reviewId, Guid userId) =>
        new(reviewId, userId, "Іван", 5, null, false, null, null, null);

    [Fact]
    public async Task Handle_ReviewNotFound_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Review?)null);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongUser_ReturnsFailure()
    {
        var review = MakePendingReview(Guid.NewGuid());
        _repo.GetByIdAsync(review.Id, default).Returns(review);

        var result = await CreateHandler().Handle(ValidCmd(review.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не можете");
    }

    [Fact]
    public async Task Handle_AlreadySubmitted_ReturnsFailure()
    {
        var reviewerId = Guid.NewGuid();
        var review = MakePendingReview(reviewerId);
        review.Submit(5, null, false, null, null, null, "Іван");
        _repo.GetByIdAsync(review.Id, default).Returns(review);

        var result = await CreateHandler().Handle(ValidCmd(review.Id, reviewerId), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_HappyPath_PublishesReviewEvent()
    {
        var reviewerId = Guid.NewGuid();
        var review = MakePendingReview(reviewerId);
        _repo.GetByIdAsync(review.Id, default).Returns(review);

        var result = await CreateHandler().Handle(ValidCmd(review.Id, reviewerId), default);

        result.IsSuccess.Should().BeTrue();
        review.Status.Should().Be(ReviewStatus.Published);
        _repo.Received(1).Update(review);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _bus.Received(1).Publish(
            Arg.Is<ReviewPublishedIntegrationEvent>(e => e.RevieweeId == review.RevieweeId),
            Arg.Any<CancellationToken>());
    }
}
