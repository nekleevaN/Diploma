using Microsoft.EntityFrameworkCore;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.ChatService.Infrastructure.Persistence;

namespace TrustMarket.ChatService.Infrastructure.Repositories;

public class ViewingRequestRepository : IViewingRequestRepository
{
    private readonly ChatDbContext _context;
    public ViewingRequestRepository(ChatDbContext context) => _context = context;

    public Task<ViewingRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.ViewingRequests.FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<List<ViewingRequest>> GetPendingFollowUpsAsync(DateTime beforeDateTime, CancellationToken ct = default)
        => _context.ViewingRequests
            .Where(v => v.Status == ViewingStatus.Accepted &&
                        !v.FollowUpSent &&
                        v.ProposedDateTime <= beforeDateTime)
            .ToListAsync(ct);

    public async Task AddAsync(ViewingRequest viewing, CancellationToken ct = default)
        => await _context.ViewingRequests.AddAsync(viewing, ct);

    public void Update(ViewingRequest viewing)
        => _context.ViewingRequests.Update(viewing);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
