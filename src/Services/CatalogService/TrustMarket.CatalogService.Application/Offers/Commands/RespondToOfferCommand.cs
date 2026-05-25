using MassTransit;
using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.CatalogService.Application.Offers.Commands;

public record RespondToOfferCommand(
    Guid OfferId,
    Guid SellerId,
    string Action,
    decimal? CounterPrice,
    string? Note) : IRequest<Result>;

public class RespondToOfferCommandHandler : IRequestHandler<RespondToOfferCommand, Result>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IAdvertisementRepository _adRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public RespondToOfferCommandHandler(
        IOfferRepository offerRepository,
        IAdvertisementRepository adRepository,
        IPublishEndpoint publishEndpoint)
    {
        _offerRepository = offerRepository;
        _adRepository = adRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(RespondToOfferCommand request, CancellationToken ct)
    {
        var offer = await _offerRepository.GetByIdAsync(request.OfferId, ct);
        if (offer is null)
            return Result.Failure("Пропозицію не знайдено");

        var ad = await _adRepository.GetByIdAsync(offer.AdvertisementId, ct);
        if (ad is null || ad.SellerId != request.SellerId)
            return Result.Failure("Доступ заборонено");

        if (offer.Status != Domain.Entities.OfferStatus.Pending &&
            offer.Status != Domain.Entities.OfferStatus.CounterOffered)
            return Result.Failure("На цю пропозицію вже відповіли");

        switch (request.Action.ToLower())
        {
            case "accept":
                offer.Accept();
                break;
            case "reject":
                offer.Reject(request.Note);
                break;
            case "counter":
                if (request.CounterPrice is null or <= 0)
                    return Result.Failure("Вкажіть зустрічну ціну");
                offer.Counter(request.CounterPrice.Value, request.Note);
                break;
            default:
                return Result.Failure("Невідома дія");
        }

        _offerRepository.Update(offer);
        await _offerRepository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new OfferRespondedIntegrationEvent(
            offer.Id, offer.AdvertisementId, offer.BuyerId,
            ad.Title, offer.Status.ToString(), offer.CounterPrice, offer.SellerNote), ct);

        return Result.Success();
    }
}
