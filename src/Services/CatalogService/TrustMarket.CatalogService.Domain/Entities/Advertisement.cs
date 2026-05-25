using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.CatalogService.Domain.Entities;

public class Advertisement : BaseEntity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string Category { get; private set; } = null!;
    public string? CategorySub { get; private set; }
    public string? CategoryItem { get; private set; }
    public string? CategoryLabel { get; private set; }
    public string? Condition { get; private set; }
    public string? Brand { get; private set; }
    public string? Size { get; private set; }
    public string? Color { get; private set; }
    public Guid SellerId { get; private set; }

    public string SellerName { get; private set; } = null!;
    public double SellerRating { get; private set; }
    public string? SellerSubMerchantId { get; private set; }

    public AdvertisementStatus Status { get; private set; }
    public List<string> ImageUrls { get; private set; } = new();

    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? LocationAddress { get; private set; }

    public bool HasLocation => Latitude.HasValue && Longitude.HasValue;

    private Advertisement() { }

    public static Advertisement Create(
        string title, string description, decimal price,
        string category, Guid sellerId, string sellerName, double sellerRating,
        string? categorySub = null, string? categoryItem = null, string? categoryLabel = null,
        string? condition = null, string? brand = null, string? size = null, string? color = null)
    {
        return new Advertisement
        {
            Title = title,
            Description = description,
            Price = price,
            Category = category,
            CategorySub = categorySub,
            CategoryItem = categoryItem,
            CategoryLabel = categoryLabel,
            Condition = condition,
            Brand = brand,
            Size = size,
            Color = color,
            SellerId = sellerId,
            SellerName = sellerName,
            SellerRating = sellerRating,
            Status = AdvertisementStatus.Active
        };
    }

    public void UpdateSellerSubMerchantId(string? subMerchantId)
    {
        SellerSubMerchantId = string.IsNullOrWhiteSpace(subMerchantId) ? null : subMerchantId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddImage(string url) => ImageUrls.Add(url);

    public void ReplaceImages(IEnumerable<string> urls)
    {
        ImageUrls.Clear();
        foreach (var url in urls) ImageUrls.Add(url);
    }

    public void UpdateDetails(string title, string description, decimal price,
        string category, string? categorySub = null, string? categoryItem = null, string? categoryLabel = null,
        string? condition = null, string? brand = null, string? size = null, string? color = null)
    {
        Title = title;
        Description = description;
        Price = price;
        Category = category;
        CategorySub = categorySub;
        CategoryItem = categoryItem;
        CategoryLabel = categoryLabel;
        Condition = condition;
        Brand = brand;
        Size = size;
        Color = color;
    }

    public void SetLocation(double? latitude, double? longitude, string? address)
    {
        Latitude = latitude;
        Longitude = longitude;
        LocationAddress = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Remove()
    {
        Status = AdvertisementStatus.Removed;
    }

    public void MarkAsReserved()
    {
        Status = AdvertisementStatus.Reserved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold()
    {
        Status = AdvertisementStatus.Sold;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsActive()
    {
        Status = AdvertisementStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum AdvertisementStatus
{
    Active = 1,
    Reserved = 2,
    Sold = 3,
    Removed = 4
}

