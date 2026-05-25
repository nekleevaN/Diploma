using FluentAssertions;
using TrustMarket.ReviewService.Domain.Entities;
using Xunit;

namespace TrustMarket.ReviewService.UnitTests.Domain;

public class ReviewTests
{
    private static Review MakePlaceholder() =>
        Review.CreatePlaceholder(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Іван", ReviewType.BuyerToSeller);

    [Fact]
    public void CreatePlaceholder_SetsPendingStatusAndFields()
    {
        var orderId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var revieweeId = Guid.NewGuid();

        var review = Review.CreatePlaceholder(orderId, reviewerId, revieweeId, "Іван", ReviewType.BuyerToSeller);

        review.Status.Should().Be(ReviewStatus.Pending);
        review.OrderId.Should().Be(orderId);
        review.ReviewerId.Should().Be(reviewerId);
        review.RevieweeId.Should().Be(revieweeId);
        review.Type.Should().Be(ReviewType.BuyerToSeller);
        review.Rating.Should().BeNull();
        review.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void CanBeSubmittedBy_ReturnsTrueForReviewerWhenPending()
    {
        var review = MakePlaceholder();

        review.CanBeSubmittedBy(review.ReviewerId).Should().BeTrue();
    }

    [Fact]
    public void CanBeSubmittedBy_ReturnsFalseForOtherUser()
    {
        var review = MakePlaceholder();

        review.CanBeSubmittedBy(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void CanBeSubmittedBy_ReturnsFalseAfterSubmit()
    {
        var review = MakePlaceholder();
        review.Submit(5, null, false, null, null, null, "Іван");

        review.CanBeSubmittedBy(review.ReviewerId).Should().BeFalse();
    }

    [Fact]
    public void Submit_SetsPublishedStatusAndFields()
    {
        var review = MakePlaceholder();

        review.Submit(4, "Чудовий продавець", false, 5, 4, 5, "Іван Оновлений");

        review.Status.Should().Be(ReviewStatus.Published);
        review.Rating.Should().Be(4);
        review.Comment.Should().Be("Чудовий продавець");
        review.ReviewerName.Should().Be("Іван Оновлений");
        review.PublishedAt.Should().NotBeNull();
        review.EditableUntil.Should().NotBeNull();
    }

    [Fact]
    public void CanBeEditedBy_ReturnsTrueWithinWindow()
    {
        var review = MakePlaceholder();
        review.Submit(5, null, false, null, null, null, "Іван");

        review.CanBeEditedBy(review.ReviewerId).Should().BeTrue();
    }

    [Fact]
    public void CanBeEditedBy_ReturnsFalseForOtherUser()
    {
        var review = MakePlaceholder();
        review.Submit(5, null, false, null, null, null, "Іван");

        review.CanBeEditedBy(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void Update_ReplacesFields()
    {
        var review = MakePlaceholder();
        review.Submit(4, "Добре", false, null, null, null, "Іван");

        review.Update(3, "Погано", true, 2, 3, 4);

        review.Rating.Should().Be(3);
        review.Comment.Should().Be("Погано");
        review.IsAnonymous.Should().BeTrue();
    }

    [Fact]
    public void Expire_SetsExpiredStatus()
    {
        var review = MakePlaceholder();

        review.Expire();

        review.Status.Should().Be(ReviewStatus.Expired);
    }
}
