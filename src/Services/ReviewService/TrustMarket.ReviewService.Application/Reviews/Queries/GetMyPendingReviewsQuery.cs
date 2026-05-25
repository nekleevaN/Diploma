using MediatR;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Queries;

public record GetMyPendingReviewsQuery(Guid ReviewerId)
    : IRequest<Result<IReadOnlyList<PendingReviewDto>>>;

public record PendingReviewDto(
    Guid ReviewId,
    Guid OrderId,
    Guid RevieweeId,
    string RevieweeName,
    string ReviewType,
    DateTime ExpiresAt);

public class GetMyPendingReviewsQueryHandler
    : IRequestHandler<GetMyPendingReviewsQuery, Result<IReadOnlyList<PendingReviewDto>>>
{
    private readonly IReviewRepository _repo;

    public GetMyPendingReviewsQueryHandler(IReviewRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<PendingReviewDto>>> Handle(
        GetMyPendingReviewsQuery req, CancellationToken ct)
    {
        var all = await _repo.GetPendingOlderThanAsync(DateTime.UtcNow.AddDays(14), ct);

        var mine = all
            .Where(r => r.ReviewerId == req.ReviewerId && r.ExpiresAt > DateTime.UtcNow)
            .Select(r => new PendingReviewDto(
                r.Id, r.OrderId, r.RevieweeId,
                r.ReviewerName,
                r.Type.ToString(),
                r.ExpiresAt))
            .ToList();

        return Result.Success<IReadOnlyList<PendingReviewDto>>(mine);
    }
}
