using MediatR;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Commands;

public record EnsureReviewPlaceholdersCommand(
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    Guid CurrentUserId,
    string BuyerName,
    string SellerName) : IRequest<Result<Guid>>;

public class EnsureReviewPlaceholdersCommandHandler
    : IRequestHandler<EnsureReviewPlaceholdersCommand, Result<Guid>>
{
    private readonly IReviewRepository _repo;

    public EnsureReviewPlaceholdersCommandHandler(IReviewRepository repo) => _repo = repo;

    public async Task<Result<Guid>> Handle(
        EnsureReviewPlaceholdersCommand req, CancellationToken ct)
    {
        if (req.CurrentUserId != req.BuyerId && req.CurrentUserId != req.SellerId)
            return Result.Failure<Guid>("Ви не є учасником цього замовлення");

        var existing = await _repo.GetByOrderIdAsync(req.OrderId, ct);

        if (existing.Count == 0)
        {
            var buyerToSeller = Review.CreatePlaceholder(
                req.OrderId, req.BuyerId, req.SellerId, req.BuyerName, ReviewType.BuyerToSeller);
            var sellerToBuyer = Review.CreatePlaceholder(
                req.OrderId, req.SellerId, req.BuyerId, req.SellerName, ReviewType.SellerToBuyer);

            await _repo.AddAsync(buyerToSeller, ct);
            await _repo.AddAsync(sellerToBuyer, ct);
            await _repo.SaveChangesAsync(ct);

            existing = new List<Review> { buyerToSeller, sellerToBuyer };
        }

        var myReview = existing.FirstOrDefault(r =>
            r.ReviewerId == req.CurrentUserId && r.Status == ReviewStatus.Pending);

        if (myReview is null)
            return Result.Failure<Guid>("Відгук вже залишено або термін минув");

        return Result.Success(myReview.Id);
    }
}
