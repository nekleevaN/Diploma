using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Advertisements.Commands;

public record DeleteAdvertisementCommand(Guid AdvertisementId, Guid SellerId) : IRequest<Result>;

public class DeleteAdvertisementCommandHandler : IRequestHandler<DeleteAdvertisementCommand, Result>
{
    private readonly IAdvertisementRepository _repository;

    public DeleteAdvertisementCommandHandler(IAdvertisementRepository repository)
        => _repository = repository;

    public async Task<Result> Handle(DeleteAdvertisementCommand request, CancellationToken ct)
    {
        var ad = await _repository.GetByIdAsync(request.AdvertisementId, ct);
        if (ad is null)
            return Result.Failure("Оголошення не знайдено");

        if (ad.SellerId != request.SellerId)
            return Result.Failure("Доступ заборонено");

        ad.Remove();
        _repository.Update(ad);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
