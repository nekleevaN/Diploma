using TrustMarket.ReviewService.Domain.Entities;

namespace TrustMarket.ReviewService.Domain.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Review>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    Task<Review?> GetPendingByReviewerAndOrderAsync(
        Guid reviewerId, Guid orderId, CancellationToken ct = default);

    Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPublishedByRevieweeAsync(
        Guid revieweeId, ReviewType? type, int page, int pageSize,
        string sort, CancellationToken ct = default);

    Task<IReadOnlyList<Review>> GetPendingExpiredAsync(
        DateTime before, CancellationToken ct = default);

    Task<IReadOnlyList<Review>> GetPendingOlderThanAsync(
        DateTime before, CancellationToken ct = default);

    Task<bool> ExistsAsync(
        Guid reviewerId, Guid revieweeId, Guid orderId, CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetSubmittedOrderIdsAsync(
        Guid reviewerId, CancellationToken ct = default);

    Task AddAsync(Review review, CancellationToken ct = default);
    void Update(Review review);
    Task SaveChangesAsync(CancellationToken ct = default);
}
