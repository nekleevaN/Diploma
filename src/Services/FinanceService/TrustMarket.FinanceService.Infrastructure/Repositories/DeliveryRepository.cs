using Microsoft.EntityFrameworkCore;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.FinanceService.Infrastructure.Persistence;

namespace TrustMarket.FinanceService.Infrastructure.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly FinanceDbContext _context;
    public DeliveryRepository(FinanceDbContext context) => _context = context;

    public Task<Delivery?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Deliveries.FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<Delivery?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId, ct);

    public async Task AddAsync(Delivery delivery, CancellationToken ct = default)
        => await _context.Deliveries.AddAsync(delivery, ct);

    public void Update(Delivery delivery)
        => _context.Deliveries.Update(delivery);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
