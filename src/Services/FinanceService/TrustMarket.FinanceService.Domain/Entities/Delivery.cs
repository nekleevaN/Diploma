using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.FinanceService.Domain.Entities;

public class Delivery : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid BuyerId { get; private set; }

    public Order? Order { get; set; }

    public string? RecipientCityRef { get; private set; }
    public string? RecipientCityName { get; private set; }
    public string? RecipientWarehouseRef { get; private set; }
    public string? RecipientWarehouseAddress { get; private set; }
    public string? RecipientName { get; private set; }
    public string? RecipientPhone { get; private set; }

    public string? SenderCityRef { get; private set; }
    public string? SenderCityName { get; private set; }
    public string? SenderWarehouseRef { get; private set; }
    public string? SenderWarehouseAddress { get; private set; }
    public string? SenderName { get; private set; }
    public string? SenderPhone { get; private set; }

    public string? TTN { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public string? TrackingStatus { get; private set; }
    public string? TrackingStatusDescription { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }

    private Delivery() { }

    public static Delivery Create(Guid orderId, Guid sellerId, Guid buyerId)
        => new()
        {
            OrderId = orderId,
            SellerId = sellerId,
            BuyerId = buyerId,
            Status = DeliveryStatus.PendingAddress
        };

    public void SetRecipientAddress(
        string cityRef, string cityName,
        string warehouseRef, string warehouseAddress,
        string recipientName, string recipientPhone)
    {
        RecipientCityRef = cityRef;
        RecipientCityName = cityName;
        RecipientWarehouseRef = warehouseRef;
        RecipientWarehouseAddress = warehouseAddress;
        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        Status = DeliveryStatus.AddressSet;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSenderAddress(
        string cityRef, string cityName,
        string warehouseRef, string warehouseAddress,
        string senderName, string senderPhone)
    {
        SenderCityRef = cityRef;
        SenderCityName = cityName;
        SenderWarehouseRef = warehouseRef;
        SenderWarehouseAddress = warehouseAddress;
        SenderName = senderName;
        SenderPhone = senderPhone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTTN(string ttn)
    {
        TTN = ttn;
        Status = DeliveryStatus.TTNCreated;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTracking(string status, string description, DateTime? estimatedDate = null)
    {
        TrackingStatus = status;
        TrackingStatusDescription = description;
        EstimatedDeliveryDate = estimatedDate;

        Status = status switch
        {
            "5" => DeliveryStatus.AtWarehouse,
            "6" => DeliveryStatus.InTransit,
            "7" => DeliveryStatus.Arrived,
            "9" => DeliveryStatus.Received,
            "14" => DeliveryStatus.Returned,
            _ => Status
        };

        if (Status == DeliveryStatus.Received)
            ActualDeliveryDate = DateTime.UtcNow;

        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsReadyForTTN =>
        !string.IsNullOrEmpty(RecipientWarehouseRef) &&
        !string.IsNullOrEmpty(SenderWarehouseRef);
}

public enum DeliveryStatus
{
    PendingAddress = 1,
    AddressSet = 2,
    TTNCreated = 3,
    AtWarehouse = 4,
    InTransit = 5,
    Arrived = 6,
    Received = 7,
    Returned = 8
}
