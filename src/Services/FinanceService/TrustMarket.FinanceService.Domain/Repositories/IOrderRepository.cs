using TrustMarket.FinanceService.Domain.Entities;

namespace TrustMarket.FinanceService.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByInvoiceIdAsync(string invoiceId, CancellationToken ct = default);
    Task<List<Order>> GetByBuyerAsync(Guid buyerId, CancellationToken ct = default);
    Task<List<Order>> GetBySellerAsync(Guid sellerId, CancellationToken ct = default);
    Task<Order?> GetByAdvertisementAndBuyerAsync(Guid advertisementId, Guid buyerId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
