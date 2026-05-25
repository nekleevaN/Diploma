using MediatR;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Commands;

public record ExpireOldReviewsCommand : IRequest<Result<int>>;

public class ExpireOldReviewsCommandHandler
    : IRequestHandler<ExpireOldReviewsCommand, Result<int>>
{
    private readonly IReviewRepository _repo;

    public ExpireOldReviewsCommandHandler(IReviewRepository repo) => _repo = repo;

    public async Task<Result<int>> Handle(
        ExpireOldReviewsCommand _, CancellationToken ct)
    {
        var expired = await _repo.GetPendingExpiredAsync(DateTime.UtcNow, ct);

        foreach (var review in expired)
        {
            review.Expire();
            _repo.Update(review);
        }

        if (expired.Count > 0)
            await _repo.SaveChangesAsync(ct);

        return Result.Success(expired.Count);
    }
}
