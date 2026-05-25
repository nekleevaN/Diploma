using TrustMarket.ChatService.Domain.Entities;

namespace TrustMarket.ChatService.Application.Abstractions;

public interface IViewingRequestRepository
{
    Task<ViewingRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ViewingRequest>> GetPendingFollowUpsAsync(DateTime beforeDateTime, CancellationToken ct = default);
    Task AddAsync(ViewingRequest viewing, CancellationToken ct = default);
    void Update(ViewingRequest viewing);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
