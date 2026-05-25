using TrustMarket.CatalogService.Domain.Entities;

namespace TrustMarket.CatalogService.Application.Abstractions;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Offer>> GetByAdAsync(Guid advertisementId, CancellationToken ct = default);
    Task<List<Offer>> GetByBuyerAsync(Guid buyerId, CancellationToken ct = default);
    Task<Offer?> GetPendingByBuyerAndAdAsync(Guid buyerId, Guid advertisementId, CancellationToken ct = default);
    Task AddAsync(Offer offer, CancellationToken ct = default);
    void Update(Offer offer);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
