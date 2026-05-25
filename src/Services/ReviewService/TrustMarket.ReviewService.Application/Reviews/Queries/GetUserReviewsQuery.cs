using MediatR;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Queries;

public record GetUserReviewsQuery(
    Guid UserId,
    string? Type,
    int Page,
    int PageSize,
    string Sort
) : IRequest<Result<ReviewsPageDto>>;

public record ReviewsPageDto(
    IReadOnlyList<ReviewDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record ReviewDto(
    Guid ReviewId,
    string ReviewType,
    int Rating,
    string? Comment,
    bool IsAnonymous,
    string? ReviewerName,
    Guid ReviewerId,
    int? DescriptionAccuracy,
    int? ShippingSpeed,
    int? Communication,
    DateTime PublishedAt);

public class GetUserReviewsQueryHandler
    : IRequestHandler<GetUserReviewsQuery, Result<ReviewsPageDto>>
{
    private readonly IReviewRepository _repo;

    public GetUserReviewsQueryHandler(IReviewRepository repo) => _repo = repo;

    public async Task<Result<ReviewsPageDto>> Handle(
        GetUserReviewsQuery req, CancellationToken ct)
    {
        ReviewType? typeFilter = req.Type switch
        {
            "seller" => ReviewType.BuyerToSeller,
            "buyer"  => ReviewType.SellerToBuyer,
            _        => null
        };

        var pageSize = Math.Clamp(req.PageSize, 1, 50);
        var page     = Math.Max(req.Page, 1);

        var (items, total) = await _repo.GetPublishedByRevieweeAsync(
            req.UserId, typeFilter, page, pageSize, req.Sort, ct);

        var dtos = items.Select(r => new ReviewDto(
            r.Id,
            r.Type.ToString(),
            r.Rating!.Value,
            r.Comment,
            r.IsAnonymous,
            r.IsAnonymous ? null : r.ReviewerName,
            r.ReviewerId,
            r.DescriptionAccuracy,
            r.ShippingSpeed,
            r.Communication,
            r.PublishedAt!.Value
        )).ToList();

        return Result.Success(new ReviewsPageDto(dtos, total, page, pageSize));
    }
}
