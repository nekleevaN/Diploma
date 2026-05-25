using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.ChatService.Domain.Entities;

public class ViewingRequest : BaseEntity
{
    public Guid ChatId { get; private set; }
    public Guid AdvertisementId { get; private set; }
    public Guid ProposerId { get; private set; }
    public Guid ResponderId { get; private set; }
    public string AdTitle { get; private set; } = null!;
    public string? LocationAddress { get; private set; }
    public DateTime ProposedDateTime { get; private set; }
    public ViewingStatus Status { get; private set; }
    public bool FollowUpSent { get; private set; }
    public string? FollowUpAction { get; private set; }

    public long? ProposerTrustedTelegramId { get; private set; }
    public long? ResponderTrustedTelegramId { get; private set; }
    public string? ProposerTrustedEmail { get; private set; }
    public string? ResponderTrustedEmail { get; private set; }

    private ViewingRequest() { }

    public static ViewingRequest Create(
        Guid chatId, Guid advertisementId,
        Guid proposerId, Guid responderId,
        string adTitle, string? locationAddress,
        DateTime proposedDateTime,
        long? proposerTrustedTelegramId = null,
        string? proposerTrustedEmail = null)
        => new()
        {
            ChatId = chatId,
            AdvertisementId = advertisementId,
            ProposerId = proposerId,
            ResponderId = responderId,
            AdTitle = adTitle,
            LocationAddress = locationAddress,
            ProposedDateTime = proposedDateTime,
            Status = ViewingStatus.Pending,
            ProposerTrustedTelegramId = proposerTrustedTelegramId,
            ProposerTrustedEmail = proposerTrustedEmail
        };

    public void SetResponderTrustedContact(long? telegramId, string? email)
    {
        ResponderTrustedTelegramId = telegramId;
        ResponderTrustedEmail = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetResponderTrustedTelegram(long? telegramId)
        => SetResponderTrustedContact(telegramId, null);

    public void Accept()
    {
        Status = ViewingStatus.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decline()
    {
        Status = ViewingStatus.Declined;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reschedule(DateTime newDateTime)
    {
        ProposedDateTime = newDateTime;
        (ProposerId, ResponderId) = (ResponderId, ProposerId);
        Status = ViewingStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFollowUpSent()
    {
        FollowUpSent = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFollowUpAction(string action)
    {
        FollowUpAction = action;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum ViewingStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3
}
