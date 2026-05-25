using MassTransit;
using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.CatalogService.Application.Offers.Commands;

public record CreateOfferCommand(
    Guid AdvertisementId,
    Guid BuyerId,
    string BuyerName,
    decimal OfferedPrice) : IRequest<Result<CreateOfferResponse>>;

public record CreateOfferResponse(Guid OfferId);

public class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, Result<CreateOfferResponse>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IAdvertisementRepository _adRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOfferCommandHandler(
        IOfferRepository offerRepository,
        IAdvertisementRepository adRepository,
        IPublishEndpoint publishEndpoint)
    {
        _offerRepository = offerRepository;
        _adRepository = adRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<CreateOfferResponse>> Handle(CreateOfferCommand request, CancellationToken ct)
    {
        var ad = await _adRepository.GetByIdAsync(request.AdvertisementId, ct);
        if (ad is null)
            return Result.Failure<CreateOfferResponse>("Оголошення не знайдено");

        if (ad.Status != AdvertisementStatus.Active)
            return Result.Failure<CreateOfferResponse>("Оголошення не активне");

        if (ad.SellerId == request.BuyerId)
            return Result.Failure<CreateOfferResponse>("Не можна торгуватись зі своїм оголошенням");

        if (request.OfferedPrice <= 0)
            return Result.Failure<CreateOfferResponse>("Ціна має бути більше нуля");

        var existing = await _offerRepository.GetPendingByBuyerAndAdAsync(request.BuyerId, request.AdvertisementId, ct);
        if (existing is not null)
            return Result.Failure<CreateOfferResponse>("У вас вже є активна пропозиція для цього оголошення");

        var offer = Offer.Create(request.AdvertisementId, request.BuyerId, request.BuyerName, request.OfferedPrice);
        await _offerRepository.AddAsync(offer, ct);
        await _offerRepository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OfferCreatedIntegrationEvent(
            offer.Id, ad.Id, ad.SellerId, request.BuyerId,
            request.BuyerName, request.OfferedPrice, ad.Title), ct);

        return Result.Success(new CreateOfferResponse(offer.Id));
    }
}
