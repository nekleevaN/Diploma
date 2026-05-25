using MediatR;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Commands;

public record CreateReviewPlaceholdersCommand(
    Guid OrderId,
    Guid BuyerId,
    Guid SellerId,
    string BuyerName,
    string SellerName) : IRequest<Result>;

public class CreateReviewPlaceholdersCommandHandler
    : IRequestHandler<CreateReviewPlaceholdersCommand, Result>
{
    private readonly IReviewRepository _repo;

    public CreateReviewPlaceholdersCommandHandler(IReviewRepository repo)
        => _repo = repo;

    public async Task<Result> Handle(
        CreateReviewPlaceholdersCommand req, CancellationToken ct)
    {
        var existing = await _repo.GetByOrderIdAsync(req.OrderId, ct);
        if (existing.Count > 0)
            return Result.Success();

        var buyerToSeller = Review.CreatePlaceholder(
            req.OrderId, req.BuyerId, req.SellerId, req.BuyerName, ReviewType.BuyerToSeller);

        var sellerToBuyer = Review.CreatePlaceholder(
            req.OrderId, req.SellerId, req.BuyerId, req.SellerName, ReviewType.SellerToBuyer);

        await _repo.AddAsync(buyerToSeller, ct);
        await _repo.AddAsync(sellerToBuyer, ct);
        await _repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}
