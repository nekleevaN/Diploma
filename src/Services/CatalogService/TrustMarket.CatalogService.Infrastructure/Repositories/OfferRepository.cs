using Microsoft.EntityFrameworkCore;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.CatalogService.Infrastructure.Persistence;

namespace TrustMarket.CatalogService.Infrastructure.Repositories;

public class OfferRepository : IOfferRepository
{
    private readonly CatalogDbContext _context;
    public OfferRepository(CatalogDbContext context) => _context = context;

    public Task<Offer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Offers.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<List<Offer>> GetByAdAsync(Guid advertisementId, CancellationToken ct = default)
        => _context.Offers
            .Where(o => o.AdvertisementId == advertisementId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public Task<List<Offer>> GetByBuyerAsync(Guid buyerId, CancellationToken ct = default)
        => _context.Offers
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public Task<Offer?> GetPendingByBuyerAndAdAsync(Guid buyerId, Guid advertisementId, CancellationToken ct = default)
        => _context.Offers.FirstOrDefaultAsync(
            o => o.BuyerId == buyerId && o.AdvertisementId == advertisementId &&
                 (o.Status == OfferStatus.Pending || o.Status == OfferStatus.CounterOffered), ct);

    public async Task AddAsync(Offer offer, CancellationToken ct = default)
        => await _context.Offers.AddAsync(offer, ct);

    public void Update(Offer offer)
        => _context.Offers.Update(offer);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
