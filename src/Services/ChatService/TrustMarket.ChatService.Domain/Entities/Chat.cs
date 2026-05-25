using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.ChatService.Domain.Entities;

public class Chat : BaseEntity
{
    public Guid BuyerId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid AdvertisementId { get; private set; }
    public string AdTitle { get; private set; } = null!;

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private Chat() { }

    public static Chat Create(Guid buyerId, Guid sellerId, Guid advertisementId, string adTitle)
    {
        return new Chat
        {
            BuyerId = buyerId,
            SellerId = sellerId,
            AdvertisementId = advertisementId,
            AdTitle = adTitle
        };
    }

    public bool IsSender(Guid userId) => userId == BuyerId || userId == SellerId;
}


public class Message : BaseEntity
{
    public Guid ChatId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = null!;
    public DateTime SentAt { get; private set; }

    public int FraudScore { get; private set; }
    public string? FraudReason { get; private set; }
    public bool IsBlocked { get; private set; }
    public bool IsFlagged { get; private set; }

    private Message() { }

    public static Message Create(Guid chatId, Guid senderId, string content,
        int fraudScore, string? fraudReason)
    {
        return new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            SentAt = DateTime.UtcNow,
            FraudScore = fraudScore,
            FraudReason = fraudReason,
            IsBlocked = fraudScore >= 70,
            IsFlagged = fraudScore >= 30 && fraudScore < 70
        };
    }
}
