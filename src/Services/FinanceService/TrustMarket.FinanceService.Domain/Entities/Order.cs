using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.FinanceService.Domain.Entities;

public class Order : BaseEntity
{
    public Guid AdvertisementId { get; private set; }
    public Guid BuyerId { get; private set; }
    public Guid SellerId { get; private set; }
    public string AdTitle { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string? InvoiceId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? LastWebhookAt { get; set; }
    public bool HasDelivery { get; private set; } = true;

    private Order() { }

    public static Order Create(Guid advertisementId, Guid buyerId, Guid sellerId, string adTitle, decimal amount, bool hasDelivery = true)
        => new()
        {
            AdvertisementId = advertisementId,
            BuyerId = buyerId,
            SellerId = sellerId,
            AdTitle = adTitle,
            Amount = amount,
            Status = OrderStatus.Pending,
            HasDelivery = hasDelivery
        };

    public void SetInvoiceId(string invoiceId)
    {
        InvoiceId = invoiceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(DateTime at)
    {
        Status = OrderStatus.Hold;
        LastWebhookAt = at;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(DateTime at)
    {
        Status = OrderStatus.Completed;
        LastWebhookAt = at;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRefunded(DateTime at)
    {
        Status = OrderStatus.Refunded;
        LastWebhookAt = at;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = OrderStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        Status = OrderStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCancelled()
    {
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsAwaitingConfirmation()
    {
        Status = OrderStatus.AwaitingConfirmation;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum OrderStatus
{
    Pending = 1,
    Hold = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5,
    Failed = 6,
    Expired = 7,
    AwaitingConfirmation = 8
}
