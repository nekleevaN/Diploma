using Microsoft.EntityFrameworkCore;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.ReviewService.Infrastructure.Persistence;

namespace TrustMarket.ReviewService.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ReviewDbContext _db;

    public ReviewRepository(ReviewDbContext db) => _db = db;

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Reviews.FindAsync([id], ct);

    public async Task<IReadOnlyList<Review>> GetByOrderIdAsync(
        Guid orderId, CancellationToken ct = default)
        => await _db.Reviews
            .Where(r => r.OrderId == orderId)
            .ToListAsync(ct);

    public async Task<Review?> GetPendingByReviewerAndOrderAsync(
        Guid reviewerId, Guid orderId, CancellationToken ct = default)
        => await _db.Reviews.FirstOrDefaultAsync(
            r => r.ReviewerId == reviewerId &&
                 r.OrderId == orderId &&
                 r.Status == ReviewStatus.Pending, ct);

    public async Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPublishedByRevieweeAsync(
        Guid revieweeId, ReviewType? type, int page, int pageSize,
        string sort, CancellationToken ct = default)
    {
        var query = _db.Reviews
            .Where(r => r.RevieweeId == revieweeId && r.Status == ReviewStatus.Published);

        if (type.HasValue)
            query = query.Where(r => r.Type == type.Value);

        var total = await query.CountAsync(ct);

        query = sort switch
        {
            "oldest"  => query.OrderBy(r => r.PublishedAt),
            "highest" => query.OrderByDescending(r => r.Rating),
            "lowest"  => query.OrderBy(r => r.Rating),
            _         => query.OrderByDescending(r => r.PublishedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Review>> GetPendingExpiredAsync(
        DateTime before, CancellationToken ct = default)
        => await _db.Reviews
            .Where(r => r.Status == ReviewStatus.Pending && r.ExpiresAt <= before)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Review>> GetPendingOlderThanAsync(
        DateTime before, CancellationToken ct = default)
        => await _db.Reviews
            .Where(r => r.Status == ReviewStatus.Pending && r.CreatedAt <= before)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        Guid reviewerId, Guid revieweeId, Guid orderId, CancellationToken ct = default)
        => await _db.Reviews.AnyAsync(
            r => r.ReviewerId == reviewerId &&
                 r.RevieweeId == revieweeId &&
                 r.OrderId == orderId, ct);

    public async Task<IReadOnlyList<Guid>> GetSubmittedOrderIdsAsync(
        Guid reviewerId, CancellationToken ct = default)
        => await _db.Reviews
            .Where(r => r.ReviewerId == reviewerId && r.Status == ReviewStatus.Published)
            .Select(r => r.OrderId)
            .Distinct()
            .ToListAsync(ct);

    public async Task AddAsync(Review review, CancellationToken ct = default)
        => await _db.Reviews.AddAsync(review, ct);

    public void Update(Review review)
        => _db.Reviews.Update(review);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
