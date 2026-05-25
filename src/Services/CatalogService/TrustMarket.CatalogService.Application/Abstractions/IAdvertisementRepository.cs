using TrustMarket.CatalogService.Domain.Entities;

namespace TrustMarket.CatalogService.Application.Abstractions;

public interface IAdvertisementRepository
{
    Task<Advertisement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Advertisement> Items, int Total)> GetPagedAsync(
        string? category, string? search, int page, int pageSize, CancellationToken ct = default,
        string? categorySub = null, string? categoryItem = null,
        string? condition = null, string? brand = null,
        decimal? priceMin = null, decimal? priceMax = null,
        string? size = null, string? color = null,
        string? sortBy = null);
    Task AddAsync(Advertisement advertisement, CancellationToken ct = default);
    void Update(Advertisement advertisement);
    Task UpdateSellerNameAsync(Guid sellerId, string displayName, CancellationToken ct = default);
    Task UpdateSellerSubMerchantIdAsync(Guid sellerId, string? subMerchantId, CancellationToken ct = default);
    Task<string?> GetSellerSubMerchantIdAsync(Guid sellerId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
