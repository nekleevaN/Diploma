using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Advertisements.Commands;

public record ReserveAdvertisementCommand(
    Guid AdvertisementId,
    Guid BuyerId) : IRequest<Result<AdReservationInfo>>;

public record AdReservationInfo(
    Guid SellerId,
    string Title,
    decimal Price,
    string? SellerSubMerchantId);

public class ReserveAdvertisementCommandHandler
    : IRequestHandler<ReserveAdvertisementCommand, Result<AdReservationInfo>>
{
    private readonly IAdvertisementRepository _repo;

    public ReserveAdvertisementCommandHandler(IAdvertisementRepository repo) => _repo = repo;

    public async Task<Result<AdReservationInfo>> Handle(
        ReserveAdvertisementCommand request, CancellationToken ct)
    {
        var ad = await _repo.GetByIdAsync(request.AdvertisementId, ct);

        if (ad is null)
            return Result.Failure<AdReservationInfo>("Оголошення не знайдено");

        if (ad.SellerId == request.BuyerId)
            return Result.Failure<AdReservationInfo>("Не можна купити власне оголошення");

        if (ad.Status != AdvertisementStatus.Active)
            return Result.Failure<AdReservationInfo>("CONFLICT:Товар вже зарезервовано або продано");

        ad.MarkAsReserved();
        _repo.Update(ad);
        await _repo.SaveChangesAsync(ct);


        return Result.Success(new AdReservationInfo(
            ad.SellerId, ad.Title, ad.Price,
            ad.SellerSubMerchantId));
    }
}


public record UnreserveAdvertisementCommand(Guid AdvertisementId) : IRequest<Result>;

public class UnreserveAdvertisementCommandHandler
    : IRequestHandler<UnreserveAdvertisementCommand, Result>
{
    private readonly IAdvertisementRepository _repo;

    public UnreserveAdvertisementCommandHandler(IAdvertisementRepository repo) => _repo = repo;

    public async Task<Result> Handle(UnreserveAdvertisementCommand request, CancellationToken ct)
    {
        var ad = await _repo.GetByIdAsync(request.AdvertisementId, ct);
        if (ad is null) return Result.Failure("Оголошення не знайдено");

        if (ad.Status == AdvertisementStatus.Reserved)
        {
            ad.MarkAsActive();
            _repo.Update(ad);
            await _repo.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
