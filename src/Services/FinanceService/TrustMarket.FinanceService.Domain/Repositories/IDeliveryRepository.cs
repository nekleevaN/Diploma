using TrustMarket.FinanceService.Domain.Entities;

namespace TrustMarket.FinanceService.Domain.Repositories;

public interface IDeliveryRepository
{
    Task<Delivery?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Delivery?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(Delivery delivery, CancellationToken ct = default);
    void Update(Delivery delivery);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
