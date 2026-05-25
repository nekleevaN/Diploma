using MediatR;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Queries;

public record GetUserRatingStatsQuery(Guid UserId)
    : IRequest<Result<UserRatingStatsDto>>;

public record RatingStatsDto(
    double Average,
    int Count,
    IReadOnlyDictionary<int, int> Distribution);

public record UserRatingStatsDto(
    RatingStatsDto AsSeller,
    RatingStatsDto AsBuyer,
    int PendingCount);

public class GetUserRatingStatsQueryHandler
    : IRequestHandler<GetUserRatingStatsQuery, Result<UserRatingStatsDto>>
{
    private readonly IReviewRepository _repo;

    public GetUserRatingStatsQueryHandler(IReviewRepository repo) => _repo = repo;

    public async Task<Result<UserRatingStatsDto>> Handle(
        GetUserRatingStatsQuery req, CancellationToken ct)
    {
        var (sellerItems, sellerTotal) = await _repo.GetPublishedByRevieweeAsync(
            req.UserId, ReviewType.BuyerToSeller, 1, int.MaxValue, "newest", ct);

        var (buyerItems, buyerTotal) = await _repo.GetPublishedByRevieweeAsync(
            req.UserId, ReviewType.SellerToBuyer, 1, int.MaxValue, "newest", ct);

        var pending = await _repo.GetPendingOlderThanAsync(DateTime.UtcNow, ct);
        var pendingCount = pending.Count(r => r.ReviewerId == req.UserId);

        return Result.Success(new UserRatingStatsDto(
            BuildStats(sellerItems),
            BuildStats(buyerItems),
            pendingCount));
    }

    private static RatingStatsDto BuildStats(IReadOnlyList<Review> reviews)
    {
        if (reviews.Count == 0)
            return new RatingStatsDto(0, 0, new Dictionary<int, int>
                { [5] = 0, [4] = 0, [3] = 0, [2] = 0, [1] = 0 });

        var distribution = Enumerable.Range(1, 5)
            .ToDictionary(k => k, k => reviews.Count(r => r.Rating == k));

        var avg = reviews.Average(r => r.Rating!.Value);

        return new RatingStatsDto(Math.Round(avg, 2), reviews.Count, distribution);
    }
}
