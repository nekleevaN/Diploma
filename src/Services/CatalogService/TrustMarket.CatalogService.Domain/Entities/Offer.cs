using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.CatalogService.Domain.Entities;

public class Offer : BaseEntity
{
    public Guid AdvertisementId { get; private set; }
    public Guid BuyerId { get; private set; }
    public string BuyerName { get; private set; } = null!;
    public decimal OfferedPrice { get; private set; }
    public OfferStatus Status { get; private set; }
    public decimal? CounterPrice { get; private set; }
    public string? SellerNote { get; private set; }

    private Offer() { }

    public static Offer Create(Guid advertisementId, Guid buyerId, string buyerName, decimal offeredPrice)
        => new()
        {
            AdvertisementId = advertisementId,
            BuyerId = buyerId,
            BuyerName = buyerName,
            OfferedPrice = offeredPrice,
            Status = OfferStatus.Pending
        };

    public void Accept()
    {
        Status = OfferStatus.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string? note = null)
    {
        Status = OfferStatus.Rejected;
        SellerNote = note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Counter(decimal counterPrice, string? note = null)
    {
        CounterPrice = counterPrice;
        SellerNote = note;
        Status = OfferStatus.CounterOffered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptCounter()
    {
        if (Status != OfferStatus.CounterOffered || CounterPrice is null)
            throw new InvalidOperationException("Offer is not in CounterOffered status");

        OfferedPrice = CounterPrice.Value;
        CounterPrice = null;
        Status = OfferStatus.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum OfferStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    CounterOffered = 4
}
