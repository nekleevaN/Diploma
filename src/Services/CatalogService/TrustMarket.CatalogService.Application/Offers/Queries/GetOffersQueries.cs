using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Offers.Queries;

public record OfferDto(
    Guid OfferId,
    Guid AdvertisementId,
    Guid BuyerId,
    string BuyerName,
    decimal OfferedPrice,
    string Status,
    decimal? CounterPrice,
    string? SellerNote,
    DateTime CreatedAt);

public record GetAdOffersQuery(Guid AdvertisementId, Guid SellerId) : IRequest<Result<List<OfferDto>>>;

public class GetAdOffersQueryHandler : IRequestHandler<GetAdOffersQuery, Result<List<OfferDto>>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IAdvertisementRepository _adRepository;

    public GetAdOffersQueryHandler(IOfferRepository offerRepository, IAdvertisementRepository adRepository)
    {
        _offerRepository = offerRepository;
        _adRepository = adRepository;
    }

    public async Task<Result<List<OfferDto>>> Handle(GetAdOffersQuery request, CancellationToken ct)
    {
        var ad = await _adRepository.GetByIdAsync(request.AdvertisementId, ct);
        if (ad is null || ad.SellerId != request.SellerId)
            return Result.Failure<List<OfferDto>>("Доступ заборонено");

        var offers = await _offerRepository.GetByAdAsync(request.AdvertisementId, ct);
        return Result.Success(offers.Select(ToDto).ToList());
    }

    private static OfferDto ToDto(Offer o) =>
        new(o.Id, o.AdvertisementId, o.BuyerId, o.BuyerName,
            o.OfferedPrice, o.Status.ToString(), o.CounterPrice, o.SellerNote, o.CreatedAt);
}

public record GetMyOffersQuery(Guid BuyerId) : IRequest<Result<List<OfferDto>>>;

public class GetMyOffersQueryHandler : IRequestHandler<GetMyOffersQuery, Result<List<OfferDto>>>
{
    private readonly IOfferRepository _offerRepository;

    public GetMyOffersQueryHandler(IOfferRepository offerRepository)
        => _offerRepository = offerRepository;

    public async Task<Result<List<OfferDto>>> Handle(GetMyOffersQuery request, CancellationToken ct)
    {
        var offers = await _offerRepository.GetByBuyerAsync(request.BuyerId, ct);
        return Result.Success(offers.Select(o =>
            new OfferDto(o.Id, o.AdvertisementId, o.BuyerId, o.BuyerName,
                o.OfferedPrice, o.Status.ToString(), o.CounterPrice, o.SellerNote, o.CreatedAt)
        ).ToList());
    }
}

public record GetPendingOffersCountQuery(Guid SellerId) : IRequest<int>;

public class GetPendingOffersCountQueryHandler : IRequestHandler<GetPendingOffersCountQuery, int>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IAdvertisementRepository _adRepository;

    public GetPendingOffersCountQueryHandler(IOfferRepository offerRepository, IAdvertisementRepository adRepository)
    {
        _offerRepository = offerRepository;
        _adRepository = adRepository;
    }

    public async Task<int> Handle(GetPendingOffersCountQuery request, CancellationToken ct)
    {
        var (ads, _) = await _adRepository.GetPagedAsync(null, null, 1, 100, ct);
        var myAdIds = ads.Where(a => a.SellerId == request.SellerId).Select(a => a.Id).ToHashSet();

        var count = 0;
        foreach (var adId in myAdIds)
        {
            var offers = await _offerRepository.GetByAdAsync(adId, ct);
            count += offers.Count(o => o.Status == OfferStatus.Pending);
        }
        return count;
    }
}
