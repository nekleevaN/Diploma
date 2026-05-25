using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.ReviewService.Domain.Entities;

public class Review : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ReviewerId { get; private set; }
    public Guid RevieweeId { get; private set; }
    public string ReviewerName { get; private set; } = null!;
    public ReviewType Type { get; private set; }
    public ReviewStatus Status { get; private set; }

    public int? Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsAnonymous { get; private set; }

    public int? DescriptionAccuracy { get; private set; }
    public int? ShippingSpeed { get; private set; }
    public int? Communication { get; private set; }

    public DateTime? PublishedAt { get; private set; }
    public DateTime? EditableUntil { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private Review() { }

    public static Review CreatePlaceholder(
        Guid orderId, Guid reviewerId, Guid revieweeId,
        string reviewerName, ReviewType type)
        => new()
        {
            OrderId = orderId,
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            ReviewerName = reviewerName,
            Type = type,
            Status = ReviewStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };

    public bool CanBeSubmittedBy(Guid userId) =>
        ReviewerId == userId && Status == ReviewStatus.Pending;

    public bool CanBeEditedBy(Guid userId) =>
        ReviewerId == userId &&
        Status == ReviewStatus.Published &&
        EditableUntil.HasValue &&
        DateTime.UtcNow <= EditableUntil.Value;

    public void Submit(
        int rating, string? comment, bool isAnonymous,
        int? descriptionAccuracy, int? shippingSpeed, int? communication,
        string reviewerName)
    {
        ReviewerName = reviewerName;
        Rating = rating;
        Comment = comment?.Trim();
        IsAnonymous = isAnonymous;
        DescriptionAccuracy = descriptionAccuracy;
        ShippingSpeed = shippingSpeed;
        Communication = communication;
        Status = ReviewStatus.Published;
        PublishedAt = DateTime.UtcNow;
        EditableUntil = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        int rating, string? comment, bool isAnonymous,
        int? descriptionAccuracy, int? shippingSpeed, int? communication)
    {
        Rating = rating;
        Comment = comment?.Trim();
        IsAnonymous = isAnonymous;
        DescriptionAccuracy = descriptionAccuracy;
        ShippingSpeed = shippingSpeed;
        Communication = communication;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = ReviewStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum ReviewStatus
{
    Pending = 1,
    Published = 2,
    Expired = 3
}

public enum ReviewType
{
    BuyerToSeller = 1,
    SellerToBuyer = 2
}
