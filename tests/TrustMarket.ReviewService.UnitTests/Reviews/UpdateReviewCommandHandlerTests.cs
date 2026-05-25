using FluentAssertions;
using NSubstitute;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using Xunit;

namespace TrustMarket.ReviewService.UnitTests.Reviews;

public class UpdateReviewCommandHandlerTests
{
    private readonly IReviewRepository _repo = Substitute.For<IReviewRepository>();

    private UpdateReviewCommandHandler CreateHandler() => new(_repo);

    private static Review MakePublishedReview(Guid reviewerId)
    {
        var review = Review.CreatePlaceholder(Guid.NewGuid(), reviewerId, Guid.NewGuid(), "Іван", ReviewType.BuyerToSeller);
        review.Submit(5, "Чудово", false, null, null, null, "Іван");
        return review;
    }

    private static UpdateReviewCommand ValidCmd(Guid reviewId, Guid userId) =>
        new(reviewId, userId, 4, "Оновлений коментар", false, null, null, null);

    [Fact]
    public async Task Handle_ReviewNotFound_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Review?)null);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_EditWindowExpired_ReturnsFailure()
    {
        var reviewerId = Guid.NewGuid();
        var review = MakePublishedReview(reviewerId);
        // CanBeEditedBy checks EditableUntil — pending review has null EditableUntil
        var pendingReview = Review.CreatePlaceholder(Guid.NewGuid(), reviewerId, Guid.NewGuid(), "Іван", ReviewType.BuyerToSeller);
        _repo.GetByIdAsync(pendingReview.Id, default).Returns(pendingReview);

        var result = await CreateHandler().Handle(ValidCmd(pendingReview.Id, reviewerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("24 години");
    }

    [Fact]
    public async Task Handle_WrongUser_ReturnsFailure()
    {
        var review = MakePublishedReview(Guid.NewGuid());
        _repo.GetByIdAsync(review.Id, default).Returns(review);

        var result = await CreateHandler().Handle(ValidCmd(review.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesFieldsAndSaves()
    {
        var reviewerId = Guid.NewGuid();
        var review = MakePublishedReview(reviewerId);
        _repo.GetByIdAsync(review.Id, default).Returns(review);

        var result = await CreateHandler().Handle(ValidCmd(review.Id, reviewerId), default);

        result.IsSuccess.Should().BeTrue();
        review.Rating.Should().Be(4);
        review.Comment.Should().Be("Оновлений коментар");
        _repo.Received(1).Update(review);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
