using MediatR;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Queries;

public record GetMySubmittedOrderIdsQuery(Guid ReviewerId)
    : IRequest<Result<IReadOnlyList<Guid>>>;

public class GetMySubmittedOrderIdsQueryHandler
    : IRequestHandler<GetMySubmittedOrderIdsQuery, Result<IReadOnlyList<Guid>>>
{
    private readonly IReviewRepository _repo;

    public GetMySubmittedOrderIdsQueryHandler(IReviewRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Guid>>> Handle(
        GetMySubmittedOrderIdsQuery req, CancellationToken ct)
    {
        var orderIds = await _repo.GetSubmittedOrderIdsAsync(req.ReviewerId, ct);
        return Result.Success(orderIds);
    }
}
