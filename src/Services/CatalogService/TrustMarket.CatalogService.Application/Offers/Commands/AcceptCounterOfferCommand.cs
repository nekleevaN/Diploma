using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Offers.Commands;

public record AcceptCounterOfferCommand(Guid OfferId, Guid BuyerId) : IRequest<Result<decimal>>;

public class AcceptCounterOfferCommandHandler : IRequestHandler<AcceptCounterOfferCommand, Result<decimal>>
{
    private readonly IOfferRepository _offerRepository;

    public AcceptCounterOfferCommandHandler(IOfferRepository offerRepository)
        => _offerRepository = offerRepository;

    public async Task<Result<decimal>> Handle(AcceptCounterOfferCommand request, CancellationToken ct)
    {
        var offer = await _offerRepository.GetByIdAsync(request.OfferId, ct);
        if (offer is null)
            return Result.Failure<decimal>("Пропозицію не знайдено");

        if (offer.BuyerId != request.BuyerId)
            return Result.Failure<decimal>("Доступ заборонено");

        if (offer.Status != OfferStatus.CounterOffered)
            return Result.Failure<decimal>("Пропозиція не є зустрічною");

        offer.AcceptCounter();
        _offerRepository.Update(offer);
        await _offerRepository.SaveChangesAsync(ct);

        return Result.Success(offer.OfferedPrice);
    }
}
