using Microsoft.EntityFrameworkCore;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.FinanceService.Infrastructure.Persistence;

namespace TrustMarket.FinanceService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly FinanceDbContext _context;
    public OrderRepository(FinanceDbContext context) => _context = context;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByInvoiceIdAsync(string invoiceId, CancellationToken ct = default)
        => _context.Orders.FirstOrDefaultAsync(o => o.InvoiceId == invoiceId, ct);

    public Task<List<Order>> GetByBuyerAsync(Guid buyerId, CancellationToken ct = default)
        => _context.Orders.Where(o => o.BuyerId == buyerId).OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public Task<List<Order>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default)
        => _context.Orders.Where(o => o.SellerId == sellerId).OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public Task<Order?> GetByAdvertisementAndBuyerAsync(Guid advertisementId, Guid buyerId, CancellationToken ct = default)
        => _context.Orders.FirstOrDefaultAsync(
            o => o.AdvertisementId == advertisementId && o.BuyerId == buyerId, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await _context.Orders.AddAsync(order, ct);

    public void Update(Order order)
        => _context.Orders.Update(order);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
