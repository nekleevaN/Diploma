namespace TrustMarket.Shared.Contracts.IntegrationEvents;

public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string Username,
    string FirstName) : IntegrationEvent;

public record UserEmailConfirmedIntegrationEvent(
    Guid UserId,
    string Email,
    string FirstName) : IntegrationEvent;

public record UserProfileUpdatedIntegrationEvent(
    Guid UserId,
    string DisplayName,
    string FullName) : IntegrationEvent;

public record UserVerifiedIntegrationEvent(
    Guid UserId,
    string BadgeType,
    DateTime VerifiedAt) : IntegrationEvent;

public record MessageSentIntegrationEvent(
    Guid MessageId,
    Guid ChatId,
    Guid SenderId,
    string Content,
    DateTime SentAt) : IntegrationEvent;

public record SuspiciousMessageDetectedIntegrationEvent(
    Guid MessageId,
    Guid SenderId,
    Guid ChatId,
    string Reason,
    int FraudScore,
    DateTime DetectedAt) : IntegrationEvent;

public record AdvertisementCreatedIntegrationEvent(
    Guid AdvertisementId,
    Guid SellerId,
    string Title,
    decimal Price) : IntegrationEvent;

public record OfferCreatedIntegrationEvent(
    Guid OfferId,
    Guid AdvertisementId,
    Guid SellerId,
    Guid BuyerId,
    string BuyerName,
    decimal OfferedPrice,
    string AdTitle) : IntegrationEvent;

public record OrderPaidIntegrationEvent(
    Guid OrderId,
    Guid AdvertisementId,
    Guid BuyerId,
    Guid SellerId,
    string AdTitle,
    decimal Amount) : IntegrationEvent;

public record OrderCompletedIntegrationEvent(
    Guid OrderId,
    Guid AdvertisementId,
    Guid BuyerId,
    Guid SellerId,
    decimal Amount) : IntegrationEvent;

public record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid AdvertisementId,
    Guid BuyerId,
    string Reason) : IntegrationEvent;

public record ViewingConfirmedIntegrationEvent(
    Guid ViewingId,
    Guid ChatId,
    Guid BuyerId,
    Guid SellerId,
    string AdTitle,
    string BuyerName,
    string SellerName,
    DateTime ViewingDateTime,
    string? LocationAddress,
    long? BuyerTrustedTelegramId,
    long? SellerTrustedTelegramId,
    string? BuyerTrustedEmail = null,
    string? SellerTrustedEmail = null) : IntegrationEvent;

public record OrderDeliveredIntegrationEvent(
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    string AdTitle,
    string BuyerName,
    string SellerName) : IntegrationEvent;

public record ReviewPublishedIntegrationEvent(
    Guid ReviewId,
    Guid RevieweeId,
    string ReviewType,
    int Rating) : IntegrationEvent;

public record ReviewReminderIntegrationEvent(
    Guid ReviewId,
    Guid ReviewerId,
    Guid RevieweeId,
    string ReviewType,
    string AdTitle) : IntegrationEvent;

public record UserPayoutMethodUpdatedIntegrationEvent(
    Guid UserId,
    string? MonobankSubMerchantId) : IntegrationEvent;

public record OfferRespondedIntegrationEvent(
    Guid OfferId,
    Guid AdvertisementId,
    Guid BuyerId,
    string AdTitle,
    string Status,
    decimal? CounterPrice,
    string? Note) : IntegrationEvent;
